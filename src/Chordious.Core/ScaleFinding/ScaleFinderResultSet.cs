// Copyright (c) Jon Thysell <http://jonthysell.com>
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Chordious.Core
{
    public class ScaleFinderResultSet
    {
        public IScaleFinderOptions ScaleFinderOptions { get; private set; }

        public int Count
        {
            get
            {
                return _results.Count;
            }
        }

        public IEnumerable<ScaleFinderResult> Results
        {
            get
            {
                EnsureSorted();
                foreach (ScaleFinderResult sfr in _results)
                {
                    yield return sfr;
                }
            }
        }

        private readonly List<ScaleFinderResult> _results;

        internal ScaleFinderResultSet(IScaleFinderOptions scaleFinderOptions)
        {
            ScaleFinderOptions = scaleFinderOptions;
            _results = new List<ScaleFinderResult>();
        }

        public void AddResult(IEnumerable<MarkPosition> marks)
        {
            if (IsValid(marks))
            {
                _results.Add(new ScaleFinderResult(this, marks));
                _sorted = false;
            }
        }

        public ScaleFinderResult ResultAt(int index)
        {
            EnsureSorted();
            return _results[index];
        }

        /// <summary>
        /// Results are sorted once when first read, rather than after every add (which made large
        /// searches quadratic). The sort is stable, so tied results keep the order they were found in.
        /// </summary>
        private void EnsureSorted()
        {
            if (!_sorted)
            {
                List<ScaleFinderResult> sorted = _results.OrderBy(result => result, ResultComparer).ToList();
                _results.Clear();
                _results.AddRange(sorted);
                _sorted = true;
            }
        }

        private static readonly IComparer<ScaleFinderResult> ResultComparer = Comparer<ScaleFinderResult>.Create((a, b) => ((IComparable)a).CompareTo(b));

        private bool _sorted = true;

        private bool IsValid(IEnumerable<MarkPosition> marks)
        {
            return MarkUtils.ValidateScale(marks, ScaleFinderOptions);
        }
    }
}
