using System;
using System.Collections;

namespace ParadoxReader
{
    public abstract class ParadoxCondition
    {
        public abstract bool IsDataOk(ParadoxRecord dataRec);
        public abstract bool IsIndexPossible(ParadoxRecord indexRec, ParadoxRecord nextRec);

        public class Compare : ParadoxCondition
        {
            public Compare(ParadoxCompareOperator op, object value, int dataFieldIndex, int indexFieldIndex)
            {
                Operator = op;
                Value = value;
                DataFieldIndex = dataFieldIndex;
                IndexFieldIndex = indexFieldIndex;
            }

            public ParadoxCompareOperator Operator { get; }
            public object Value { get; }

            public int DataFieldIndex { get; }
            public int IndexFieldIndex { get; }

            public override bool IsDataOk(ParadoxRecord dataRec)
            {
                object val = dataRec.DataValues[DataFieldIndex];
                int comp = Comparer.Default.Compare(val, Value);
                switch (Operator)
                {
                    case ParadoxCompareOperator.Equal:
                        return comp == 0;
                    case ParadoxCompareOperator.NotEqual:
                        return comp != 0;
                    case ParadoxCompareOperator.Greater:
                        return comp > 0;
                    case ParadoxCompareOperator.GreaterOrEqual:
                        return comp >= 0;
                    case ParadoxCompareOperator.Less:
                        return comp < 0;
                    case ParadoxCompareOperator.LessOrEqual:
                        return comp <= 0;
                    default:
                        throw new NotSupportedException();
                }
            }

            public override bool IsIndexPossible(ParadoxRecord indexRec, ParadoxRecord nextRec)
            {
                object val1 = indexRec.DataValues[DataFieldIndex];
                int comp1 = Comparer.Default.Compare(val1, Value);
                int comp2;
                if (nextRec != null)
                {
                    object val2 = nextRec.DataValues[DataFieldIndex];
                    comp2 = Comparer.Default.Compare(val2, Value);
                }
                else
                {
                    comp2 = 1; // last index range ends in infinite
                }

                switch (Operator)
                {
                    case ParadoxCompareOperator.Equal:
                        return comp1 <= 0 && comp2 >= 0;
                    case ParadoxCompareOperator.NotEqual:
                        return comp1 > 0 || comp2 < 0;
                    case ParadoxCompareOperator.Greater:
                        return comp2 > 0;
                    case ParadoxCompareOperator.GreaterOrEqual:
                        return comp2 >= 0;
                    case ParadoxCompareOperator.Less:
                        return comp1 < 0;
                    case ParadoxCompareOperator.LessOrEqual:
                        return comp1 <= 0;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        public abstract class Multiple : ParadoxCondition
        {
            protected Multiple(ParadoxCondition[] subConditions)
            {
                SubConditions = subConditions;
            }

            protected ParadoxCondition[] SubConditions { get; }

            public override bool IsDataOk(ParadoxRecord dataRec)
            {
                return Test(c => c.IsDataOk(dataRec));
            }

            public override bool IsIndexPossible(ParadoxRecord indexRec, ParadoxRecord nextRec)
            {
                return Test(c => c.IsIndexPossible(indexRec, nextRec));
            }

            protected abstract bool Test(Predicate<ParadoxCondition> test);
        }

        public class LogicalAnd : Multiple
        {
            public LogicalAnd(params ParadoxCondition[] subConditions) : base(subConditions)
            {
            }

            protected override bool Test(Predicate<ParadoxCondition> test)
            {
                foreach (ParadoxCondition subCondition in SubConditions)
                {
                    if (!test(subCondition))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public class LogicalOr : Multiple
        {
            public LogicalOr(params ParadoxCondition[] subConditions) : base(subConditions)
            {
            }

            protected override bool Test(Predicate<ParadoxCondition> test)
            {
                foreach (ParadoxCondition subCondition in SubConditions)
                {
                    if (test(subCondition))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
