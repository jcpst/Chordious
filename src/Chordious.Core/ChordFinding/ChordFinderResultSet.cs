// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Chordious.Core
{
    public class ChordFinderResultSet
    {
        public IChordFinderOptions ChordFinderOptions { get; private set; }

        public int Count
        {
            get
            {
                return _results.Count;
            }
        }

        public IEnumerable<IChordFinderResult> Results
        {
            get
            {
                EnsureSorted();
                foreach (ChordFinderResult cfr in _results)
                {
                    yield return cfr;
                }
            }
        }

        private readonly List<IChordFinderResult> _results;

        internal ChordFinderResultSet(IChordFinderOptions chordFinderOptions)
        {
            ChordFinderOptions = chordFinderOptions;
            _results = new List<IChordFinderResult>();
        }

        public void AddResult(int[] marks)
        {
            if (IsValid(marks))
            {
                _results.Add(new ChordFinderResult(this, marks));
                _sorted = false;
            }
        }

        public IChordFinderResult ResultAt(int index)
        {
            EnsureSorted();
            return _results[index];
        }

        public Diagram DiagramAt(int index, ChordFinderStyle chordFinderStyle)
        {
            IChordFinderResult result = ResultAt(index);
            return result.ToDiagram(chordFinderStyle);
        }

        public DiagramCollection ToDiagramCollection(ChordFinderStyle chordFinderStyle)
        {
            DiagramCollection collection = new DiagramCollection(chordFinderStyle.Style);

            EnsureSorted();
            foreach (ChordFinderResult result in _results)
            {
                collection.Add(result.ToDiagram(chordFinderStyle));
            }

            return collection;
        }

        /// <summary>
        /// Results are sorted once when first read, rather than after every add (which made large
        /// searches quadratic). The sort is stable, so tied results keep the order they were found in.
        /// </summary>
        private void EnsureSorted()
        {
            if (!_sorted)
            {
                List<IChordFinderResult> sorted = _results.OrderBy(result => result, ResultComparer).ToList();
                _results.Clear();
                _results.AddRange(sorted);
                _sorted = true;
            }
        }

        private static readonly IComparer<IChordFinderResult> ResultComparer = Comparer<IChordFinderResult>.Create((a, b) => ((IComparable)a).CompareTo(b));

        private bool _sorted = true;

        private bool IsValid(int[] marks)
        {
            return MarkUtils.ValidateChord(marks, ChordFinderOptions);
        }
    }
}
