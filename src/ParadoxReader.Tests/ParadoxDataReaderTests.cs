using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace ParadoxReader.Tests
{
    public class ParadoxDataReaderTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ParadoxDataReaderTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
            _dbPath = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location), "Data");
            _table = new ParadoxTable(_dbPath, "zakazky");
        }

        private readonly string _dbPath;
        private readonly ParadoxTable _table;

        [Fact]
        public void Read_10_records_by_index_key_range_1750_to_1760()
        {
            ParadoxPrimaryKey index = new ParadoxPrimaryKey(_table, Path.Combine(_dbPath, "zakazky.PX"));
            ParadoxCondition.LogicalAnd condition =
                new ParadoxCondition.LogicalAnd(
                    new ParadoxCondition.Compare(ParadoxCompareOperator.GreaterOrEqual, 1750, 0, 0),
                    new ParadoxCondition.Compare(ParadoxCompareOperator.LessOrEqual, 1760, 0, 0));
            IEnumerable<ParadoxRecord> qry = index.Enumerate(condition);
            ParadoxDataReader rdr = new ParadoxDataReader(_table, qry);
            int recIndex = 1;
            while (rdr.Read())
            {
                _testOutputHelper.WriteLine("Record #{0}", recIndex++);
                for (int i = 0; i < rdr.FieldCount; i++)
                {
                    _testOutputHelper.WriteLine("    {0} = {1}", rdr.GetName(i), rdr[i]);
                }
            }

            recIndex.Should().Be(12);
        }

        [Fact]
        public void Sequential_read_first_10_records_from_start()
        {
            int recIndex = 1;
            foreach (ParadoxRecord rec in _table.Enumerate())
            {
                _testOutputHelper.WriteLine("Record #{0}", recIndex++);
                for (int i = 0; i < _table.FieldCount; i++)
                {
                    _testOutputHelper.WriteLine("    {0} = {1}", _table.FieldNames[i], rec.DataValues[i]);
                }

                if (recIndex > 10)
                {
                    break;
                }
            }

            recIndex.Should().Be(11);
        }
    }
}
