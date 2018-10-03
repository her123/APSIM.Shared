﻿namespace APSIM.Shared.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// An indexed DataTable helper class. An index (column name / value pairs) is applied
    /// to the datatable. Thereafter, scalars or a vector of values can then be 'set' into 
    /// the table for the applied index. Rows and columns will be automatically added as 
    /// required when data is 'set' into the table.
    /// </summary>
    public class IndexedDataTable
    {
        /// <summary>Internal data view that implements the index</summary>
        DataView view;

        /// <summary>List of column names that make up the index</summary>
        private IList<string> indexColumnNames;

        /// <summary>List of column values that make up the index</summary>
        private IList<object> indexColumnValues;

        /// <summary>Constructor</summary>
        /// <param name="table">The data table to work on</param>
        /// <param name="indexColumns">The names of the index columns</param>
        public IndexedDataTable(DataTable table, IList<string> indexColumns)
        {
            indexColumnNames = indexColumns;
            view = new DataView(table);
        }

        /// <summary>Set the tables unique index</summary>
        /// <param name="indexValues">List of column names that make up the index</param>
        public void SetIndex(object[] indexValues)
        {
            indexColumnValues = indexValues;
            if (indexColumnValues.Count != indexColumnNames.Count)
                throw new Exception("Invalid number of index values passed to IndexedDataTable.SetIndex method");

            EnsureColumnsExist(indexColumnNames, indexColumnValues);

            string filter = null;
            for (int i = 0; i < indexColumnNames.Count; i++)
            {
                if (filter != null)
                    filter += " AND ";
                filter += indexColumnNames[i] + " = ";
                if (indexColumnValues[i] is string)
                    filter += "'" + indexColumnValues[i] + "'";
                else
                    filter += indexColumnValues[i];
            }
            view.RowFilter = filter;
        }

        /// <summary>Set a value for the specified column for all rows that match the current index</summary>
        /// <param name="columnName">The column name</param>
        /// <param name="value">The value to insert into the table</param>
        public void Set<T>(string columnName, T value)
        {
            EnsureColumnExists(columnName, value);
            EnsureNumRows(1);
            foreach (DataRowView row in view)
                row[columnName] = value;
        }

        /// <summary>Set a series of values for the specified column for all rows that match the current index</summary>
        /// <param name="columnName">The column name</param>
        /// <param name="values">The values to insert into the table</param>
        public void SetValues<T>(string columnName, IList<T> values)
        {
            EnsureColumnExists(columnName, values[0]);
            EnsureNumRows(values.Count);
            for (int i = 0; i < values.Count; i++)
                view[i][columnName] = values[i];
        }

        /// <summary>Ensure columns exist in data table</summary>
        /// <param name="columnNames">The column names</param>
        /// <param name="columnValues">The values used to determine column data types</param>
        private void EnsureColumnsExist(IList<string> columnNames, IList<object> columnValues)
        {
            for (int i = 0; i < columnNames.Count; i++)
            {
                if (!view.Table.Columns.Contains(columnNames[i]))
                    view.Table.Columns.Add(columnNames[i], columnValues[i].GetType());
            }
        }

        /// <summary>Ensure a column exists in data table</summary>
        /// <param name="columnName">The column name</param>
        /// <param name="columnValue">The value used to determine column data type</param>
        private void EnsureColumnExists(string columnName, object columnValue)
        {
            if (!view.Table.Columns.Contains(columnName))
                view.Table.Columns.Add(columnName, columnValue.GetType());
        }

        /// <summary>Ensure a specified number of rows exist</summary>
        /// <param name="numRows">The number of rows required in the data table</param>
        private void EnsureNumRows(int numRows)
        {
            while (view.Count < numRows)
            {
                DataRow newRow = view.Table.NewRow();
                view.Table.Rows.Add(newRow);
                for (int i = 0; i < indexColumnNames.Count; i++)
                    newRow[indexColumnNames[i]] = indexColumnValues[i];
            }
        }
    }
}
