using System;
using System.Linq;
using System.Collections.Generic;
using static Bv.Tools.Utils.GuardClauses;

namespace Bv.Tools.Tables
{
    public class SimpleTableMapper
    {
        public static SimpleTableMapper<B> CreateTableMapper<B>(IEnumerable<B> instance)
        {
            return new SimpleTableMapper<B>(instance);
        }
    }

     public class SimpleTableMapper<T> : ITableMapper<T>
     {
        private IEnumerable<T> Source;

        private List<WriterMapper<T>> mappers = new List<WriterMapper<T>>();

        public SimpleTableMapper(IEnumerable<T> e)
        {
            this.Source = IsNotNull(e, "IEnumerable should not be null");
        }

        public ITableMapper<T> Field(string name, Func<T, string> f)
        {
            this.mappers.Add(new WriterMapper<T>(name, f));
            return this;
        }

        public void WriteTo(ITableFactory source)
        {
            IsNotNull(source,"Source should be provided");

            List<List<string>> content = PreLoadContent();
            ITable table = CreateTableBasedOnColumnWidths(source,content);
           
            WriteHeaderTo(table);
            WriteRowsTo(table,content);
           
            table.WriteEnd();
        }
         
        private List<List<string>> PreLoadContent()
        {
            List<List<string>> content = new List<List<string>>();
            
            foreach (var data in this.Source)
            {
                content.Add(this.mappers.Select((mapper) => mapper.Map(data)).ToList());
            }

            return content;
        }

        private ITable CreateTableBasedOnColumnWidths(ITableFactory source,List<List<string>> contents)
        {
            ITable table = source.NewTable();

            foreach (int width in CalculateColumnWidths(contents))
            {
                table.Field(width);
            }

            return table;
        }

        private List<int> CalculateColumnWidths(List<List<string>> content)
        {
            List<int> fieldWidths = this.mappers.Select((m) => m.ColumnName.Length).ToList();

            foreach(var row in content) {
                foreach(var cell in Enumerable.Select(row, (data, index) => new { index, data.Length })) {
                    if (cell.Length > fieldWidths[(int)cell.index])
                    {
                         fieldWidths[cell.index] = (cell.Length);
                    }
                }
            }

            return fieldWidths;
        }

        private void WriteHeaderTo(ITable table)
        {
            table.WriteHeader(this.mappers.Select((c) => c.ColumnName).ToArray());
        }

        private void WriteRowsTo(ITable table, List<List<string>> rows)
        {
            foreach (var row in rows)
            {
                table.WriteRow(row.ToArray());
            }
        }
     }

    internal class WriterMapper<T>
    {
        internal string ColumnName;
        internal Func<T, string> Map;

        internal WriterMapper(string column, Func<T, string> map)
        {
            this.ColumnName = (column == null ? "": column);
            this.Map = IsNotNull(map,"Mapper cannot be null");
        }
    }
}