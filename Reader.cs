using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;


namespace nthings2.src.app
{
    public class Reader : IEnumerable<IDataReader>
    {
        IDataReader reader;
        Func<IDataReader, bool> predicate;

        public Reader(IDataReader reader, Func<IDataReader, bool> predicate = null)
        {
            this.reader = reader;
            this.predicate = predicate ?? (r => true);
        }

        public Reader(Reader reader, Func<IDataReader, bool> predicate)
            : this (reader.reader, predicate)
        {

        }

        public object this[string ColumnName]
        {
            get
            {
                return reader[ColumnName];
            }
        }

        public IEnumerator<IDataReader> GetEnumerator()
        {
            if (predicate (reader))
            {
                yield return reader;

                while (reader.Read () & predicate (reader))
                    yield return reader;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator ();
        }

        
    }

    interface Person
    {
        string Name { get; set; }
        int ID { get; set; }
    }  
}