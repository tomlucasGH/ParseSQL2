using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;



namespace ParseSQL2.Models
{
    public class TableList
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }
        public int queryID { get; set; }
        public string DB { get; set; }

        public string owner { get; set; }
        public string TableName { get; set; }
        public string AliasName { get; set; }

    }
    public class QueryMaster
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }
        public string QueryText { get; set; }
        public int customerid { get; set; }
        public virtual ICollection<TableList> Tables { get; set; }
        public virtual ICollection<TableList> SelectColumns { get; set; }
    }
    public class SelectColumns
    {

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }

        public int QueryID { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
    }
    public class WhereClause
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }
        public int QueryID { get; set; }
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string comparison_operator { get; set; }
        public string comparison_value { get; set; }
        public string function_string { get; set; }
        public string function_name { get; set; }
    }
    public class ColumnIdentifiers
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }
        public int asciistart { get; set; }
        public int asciiend { get; set; }
    }
    public class datasources
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int ID { get; set; }
        public string type { get; set; }
        public string connection { get; set; }
        public string name { get; set; }
    }
}