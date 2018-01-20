using System.Collections.Generic;

namespace ParadoxReader
{
    public class ParadoxPrimaryKey : ParadoxFile
    {
        private readonly ParadoxTable _table;

        public ParadoxPrimaryKey(ParadoxTable table, string filePath)
            : base(filePath)
        {
            _table = table;
        }

        public IEnumerable<ParadoxRecord> Enumerate(ParadoxCondition condition)
        {
            return Enumerate(condition, (ushort) (PxRootBlockId - 1), PxLevelCount);
        }

        private IEnumerable<ParadoxRecord> Enumerate(ParadoxCondition condition, ushort blockId, int indexLevel)
        {
            if (indexLevel == 0)
            {
                DataBlock block = _table.GetBlock(blockId);
                for (int i = 0; i < block.RecordCount; i++)
                {
                    ParadoxRecord rec = block[i];
                    if (condition.IsDataOk(rec))
                    {
                        yield return rec;
                    }
                }
            }
            else
            {
                DataBlock block = GetBlock(blockId);
                int blockIdFldIndex = FieldCount - 3;
                for (int i = 0; i < block.RecordCount; i++)
                {
                    ParadoxRecord rec = block[i];
                    if (condition.IsIndexPossible(rec, i < block.RecordCount - 1 ? block[i + 1] : null))
                    {
                        IEnumerable<ParadoxRecord> qry = Enumerate(condition, (ushort) ((short) rec.DataValues[blockIdFldIndex] - 1), indexLevel - 1);
                        foreach (ParadoxRecord dataRec in qry)
                        {
                            yield return dataRec;
                        }
                    }
                }
            }
        }
    }
}
