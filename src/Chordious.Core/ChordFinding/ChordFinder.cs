// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Chordious.Core
{
    public class ChordFinder
    {
        public static ChordFinderResultSet FindChords(IChordFinderOptions chordFinderOptions)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            Task<ChordFinderResultSet> task = FindChordsAsync(chordFinderOptions, cts.Token);
            task.Wait();

            return task.Result;
        }

        public static async Task<ChordFinderResultSet> FindChordsAsync(IChordFinderOptions chordFinderOptions, CancellationToken cancelToken)
        {
            if (chordFinderOptions is null)
            {
                throw new ArgumentNullException(nameof(chordFinderOptions));
            }

            InternalNote root = NoteUtils.ToInternalNote(chordFinderOptions.RootNote);
            IChordQuality chordQuality = chordFinderOptions.ChordQuality;

            InternalNote[] notesInChord = NamedInterval.GetNotes(root, chordQuality.Intervals);

            ChordFinderResultSet results = new ChordFinderResultSet(chordFinderOptions);

            if (cancelToken.IsCancellationRequested)
            {
                return results;
            }

            // The search is CPU-bound, so run it synchronously on a worker thread rather than as
            // a deeply nested chain of async calls (which allocates a state machine per node)
            await Task.Run(() => FindAllChords(results, null, notesInChord, 0, int.MaxValue, int.MinValue, chordFinderOptions, cancelToken), cancelToken).ConfigureAwait(false);

            return results;
        }

        /// <param name="minFret">Lowest fretted (> 0) fret so far, for pruning by reach.</param>
        /// <param name="maxFret">Highest fretted (> 0) fret so far, for pruning by reach.</param>
        private static void FindAllChords(ChordFinderResultSet results, NoteNode noteNode, InternalNote[] targetNotes, int str, int minFret, int maxFret, IChordFinderOptions chordFinderOptions, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
            {
                return;
            }

            IInstrument instrument = chordFinderOptions.Instrument;

            if (str == instrument.NumStrings) // Build a chord result
            {
                int[] marks = new int[instrument.NumStrings];

                NoteNode nn = noteNode;
                str--;

                bool[] hasNotes = new bool[targetNotes.Length];

                // Walk back up the tree to set the marks on the result and flag each target note
                while (nn is not null)
                {
                    marks[str] = nn.Fret;

                    for (int i = 0; i < targetNotes.Length; i++)
                    {
                        if (nn.Note == targetNotes[i])
                        {
                            hasNotes[i] = true;
                        }
                    }

                    nn = nn.Parent;
                    str--;
                }

                InternalNote rootNote = NoteUtils.ToInternalNote(chordFinderOptions.RootNote);

                // Check for necessary notes

                bool rootInTarget = false;

                bool hasAllNotes = true;
                bool hasAllNonRootNotes = true;

                bool hasRootNote = false;

                for (int i = 0; i < hasNotes.Length; i++)
                {
                    hasAllNotes = hasAllNotes && hasNotes[i];

                    if (targetNotes[i] == rootNote)
                    {
                        rootInTarget = true;

                        if (hasNotes[i])
                        {
                            hasRootNote = true;
                        }
                    }
                    else
                    {
                        hasAllNonRootNotes = hasAllNonRootNotes && hasNotes[i];
                    }
                }

                if (hasAllNotes // Allways accept: all notes present
                    || (chordFinderOptions.AllowRootlessChords && (chordFinderOptions.AllowPartialChords || hasAllNonRootNotes)) // If allow rootless, accept: anything with allow partial, or all non-root notes present
                    || (!chordFinderOptions.AllowRootlessChords && chordFinderOptions.AllowPartialChords && (!rootInTarget || (rootInTarget && hasRootNote)))) // If don't allow rootless and allow partial, accept: no root in target, or root in target is present
                {
                    results.AddResult(marks);
                }
            }
            else // Keep building the tree
            {
                // Look at the muted string
                if (chordFinderOptions.AllowMutedStrings)
                {
                    NoteNode muted = new NoteNode
                    {
                        Parent = noteNode
                    };
                    FindAllChords(results, muted, targetNotes, str + 1, minFret, maxFret, chordFinderOptions, cancelToken);
                }

                // Look at all the notes on the string
                int startingFret = chordFinderOptions.AllowOpenStrings ? 0 : 1;
                for (int fret = startingFret; fret <= chordFinderOptions.MaxFret; fret++)
                {
                    InternalNote note = NoteUtils.Shift(chordFinderOptions.Tuning.RootNotes[str].InternalNote, fret);

                    // See if the note is a target note
                    for (int i = 0; i < targetNotes.Length; i++)
                    {
                        // If it's a target note add it and search on the next string
                        if (note == targetNotes[i])
                        {
                            int childMinFret = minFret;
                            int childMaxFret = maxFret;

                            // Only fretted notes count toward reach (see MarkUtils.ChordAnalysis)
                            if (fret > 0)
                            {
                                childMinFret = Math.Min(minFret, fret);
                                childMaxFret = Math.Max(maxFret, fret);

                                // Reach only grows as strings are added, so this shape can never become valid
                                if (childMaxFret - childMinFret + 1 > chordFinderOptions.MaxReach)
                                {
                                    break;
                                }
                            }

                            NoteNode child = new NoteNode(fret, note, noteNode);
                            FindAllChords(results, child, targetNotes, str + 1, childMinFret, childMaxFret, chordFinderOptions, cancelToken);
                            break;
                        }
                    }
                }
            }
        }

        protected class NoteNode
        {
            public int Fret { get; set; }
            public InternalNote? Note { get; set; }
            public NoteNode Parent { get; set; }

            public NoteNode(int fret, InternalNote? note, NoteNode parent)
            {
                Fret = fret;
                Note = note;
                Parent = parent;
            }

            public NoteNode() : this(-1, null, null) { }

            public NoteNode(NoteNode parent) : this(-1, null, parent) { }
        }
    }
}
