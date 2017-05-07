using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using ParseSQL2.DAL;

namespace Helpers
{
    public class Parser
    {

        /*GetFilterCriteriaFromQueryString method takes input as parameters, query string and the list of tables identified previously in 
        the GetTableNamesFromQueryString method */
        public static List<wherestruct> GetFilterCriteriaFromQueryString(string query, List<outputstruct> tablelist)
        {
            wherestruct outputdata = new wherestruct();
            var output = new List<wherestruct>();
            var sb = new StringBuilder();
            var parser = new TSql120Parser(true);

            //Identifier types used to recognize specific elements in query which are not reserved keywords such as "SELECT", "FROM", "JOIN"
            //If an Identifier type is found in query string, it is part of a user supplied search value or table name
            var identifierTokenTypes = new[]
            {
            TSqlTokenType.Identifier,
            TSqlTokenType.QuotedIdentifier,
            TSqlTokenType.Integer,
            TSqlTokenType.AsciiStringLiteral
        };
            var whereTokenTypes = new[]
    {
            TSqlTokenType.Where,
            TSqlTokenType.And
        };
            var ComparisonOperators = new[]
            {
            TSqlTokenType.In,
            TSqlTokenType.EqualsSign,
            TSqlTokenType.Not,
            TSqlTokenType.GreaterThan,
            TSqlTokenType.LessThan,
            TSqlTokenType.Like
            };
            using (System.IO.TextReader tReader = new System.IO.StringReader(query))
            {
                IList<ParseError> errors;

                /*Each query token represents a distinct word in the query.  Below FOR loop identifies start of where clause
                   by matchint to "WhereTokenTypes" array and then from there an inner loop is used to identify specific 
                   search arguments in WHERE clause*/

                var queryTokens = parser.GetTokenStream(tReader, out errors);
                for (var i = 0; i < queryTokens.Count; i++)
                {
                    if (whereTokenTypes.Contains(queryTokens[i].TokenType))
                    {
                        if (outputdata.Table != null)
                        {
                            output.Add(outputdata);
                            outputdata.Table = null;
                            outputdata.Column = null;
                            outputdata.comparison_operator = null;
                            outputdata.comparison_value = null;
                        }
                        //Identify individual search arguments
                        for (var j = i + 1; j < queryTokens.Count; j++)
                        {
                            if (queryTokens[j].Text == null)
                            {
                                break;
                            }
                            //Reached end of WHERE query if reached the GROUP BY or ORDER BY clauses
                            if (queryTokens[j].Text.ToUpper() == "GROUP" || queryTokens[j].Text.ToUpper() == "ORDER" || queryTokens[j].Text.ToUpper() == "AND")
                            {
                                break;
                            }
                            // outputlist.Clear();
                            if (queryTokens[j].TokenType == TSqlTokenType.WhiteSpace)
                            {
                                continue;
                            }

                            //Checking if search argument by matching to Identifier or comparison operator
                            if (identifierTokenTypes.Contains(queryTokens[j].TokenType) || ComparisonOperators.Contains(queryTokens[j].TokenType))
                            {
                                sb.Clear();

                                //Provided Quoted Identifier "[]" for user supplied value such as table name, etc.  But not for comparison operator
                                //Maybe the [] are not needed to pass through query to document db.  If not, we can comment this part out.
                                if (identifierTokenTypes.Contains(queryTokens[j].TokenType) && queryTokens[j].TokenType != TSqlTokenType.Integer && queryTokens[j].TokenType != TSqlTokenType.AsciiStringLiteral)
                                {
                                    GetQuotedIdentifier(queryTokens[j], sb);
                                }
                                //case where alias and column provided in format "alias.column"
                                if (queryTokens[j + 1].TokenType == TSqlTokenType.Dot && identifierTokenTypes.Contains(queryTokens[j + 2].TokenType)) //Alias or Table . ColumnName
                                {
                                    foreach (outputstruct v in tablelist)
                                    {
                                        if (v.Table == sb.ToString())
                                        {
                                            outputdata.Table = v.Table;
                                            break;
                                        }
                                        //If query is using Alias in Where clause, need to sub in Table Name from outputstruct struct
                                        if (v.Alias == sb.ToString())
                                        {
                                            outputdata.Table = v.Table;
                                            break;
                                        }
                                    }
                                    //If above check failed to identify a table name, then need to break out of loop and move to next argument
                                    if (outputdata.Table == null)
                                    {
                                        break;
                                    }

                                    //If format is alias.columnname, then column is 2 positions "J+2" ahead of alias identified above
                                    else
                                    {
                                        outputdata.Column = queryTokens[j + 2].Text;

                                        //Move ahead 3 spots to look for next search argument
                                        j += 3;
                                    }

                                }

                                //Check if argument is a comparison operator (=,<,>,in,like)
                                if (ComparisonOperators.Contains(queryTokens[j].TokenType))
                                {
                                    outputdata.comparison_operator = queryTokens[j].Text;
                                    j += 1;
                                }

                                //If match identifiertokentype, this is user supplied value, so must be value being compared to
                                //logic assumes that a user supplied value with "." is alias.column.  If no "." included, must be a search value
                                if (identifierTokenTypes.Contains(queryTokens[j].TokenType))
                                {
                                    if (outputdata.comparison_value != null)
                                    {
                                        outputdata.comparison_value = String.Concat(outputdata.comparison_value, ",", queryTokens[j].Text);
                                    }
                                    else
                                    {
                                        outputdata.comparison_value = queryTokens[j].Text;
                                    }
                                }

                            }

                        }

                    }
                }
            }

            //After exiting FOR Loop above, need to insert remaining outputdata struct into the output list
            if (outputdata.Table != null)
            {
                output.Add(outputdata);
                outputdata.Table = null;
                outputdata.Column = null;
                outputdata.comparison_operator = null;
                outputdata.comparison_value = null;
            }
            return output;

        }
        public static List<columnstruct> FindColumns(string query)
        {
            columnstruct column = new columnstruct();
            List<columnstruct> columnlist = new List<columnstruct>();

            string[] columns = query.Split(' ');
            foreach (string n in columns)
            {
                string val = n.Replace(",", "");
                if (n.Contains(" "))
                {
                    val = n.Substring(n.IndexOf(" ") + 1);
                }
                if (val.Contains("."))
                {
                    string[] output = val.Split('.');
                    column.Alias = getNestedColumn(output[0],"reverse");
                    column.Column = getNestedColumn(output[1],"forward");
                    columnlist.Add(column);
                }

            }
            return columnlist;
        }

        //Method used to identify table names contained within query string passed in as a parameter
        public static List<outputstruct> GetTableNamesFromQueryString(string query)
        {
            outputstruct outputdata = new outputstruct();
            var output = new List<outputstruct>();
            var sb = new StringBuilder();
            var parser = new TSql120Parser(true);

            var fromTokenTypes = new[]
            {
            TSqlTokenType.From,
            TSqlTokenType.Join
        };

            var identifierTokenTypes = new[]
            {
            TSqlTokenType.Identifier,
            TSqlTokenType.QuotedIdentifier
        };

            using (System.IO.TextReader tReader = new System.IO.StringReader(query))
            {


                IList<ParseError> errors;

                //queryTokens contains separate element for each word in query
                var queryTokens = parser.GetTokenStream(tReader, out errors);


                //Identify start of FROM clause by matching to fromTokenTypes
                for (var i = 0; i < queryTokens.Count; i++)
                {

                    int newclause = 1;
                    if (fromTokenTypes.Contains(queryTokens[i].TokenType))
                    {
                        if (outputdata.DB != null)
                        {
                            output.Add(outputdata);
                            outputdata.DB = null;
                            outputdata.Table = null;
                            outputdata.Owner = null;
                            outputdata.Alias = null;
                        }

                        //Inner loop to identify specific tables and aliases included in FROM clause
                        //If encountered another "FROM" clause or the start of the "WHERE" or "GROUP" or "ORDER" clause, then have reached end of FROM 
                        for (var j = i + 1; j < queryTokens.Count; j++)
                        {
                            if (fromTokenTypes.Contains(queryTokens[j].TokenType))
                            {
                                break;
                            }
                            if (queryTokens[j].Text.ToUpper() == "WHERE" || queryTokens[j].Text.ToUpper() == "GROUP" || queryTokens[j].Text.ToUpper() == "ORDER" || queryTokens[j].Text.ToUpper() == "ON")
                            {
                                break;
                            }

                            if (queryTokens[j].TokenType == TSqlTokenType.WhiteSpace)
                            {
                                continue;
                            }

                            //User supplied value that is not a reserved word such as "JOIN" or "ON".  Indicates table, alias, or column name
                            if (identifierTokenTypes.Contains(queryTokens[j].TokenType))
                            {
                                sb.Clear();
                                GetQuotedIdentifier(queryTokens[j], sb);

                                //query can use format mdyb..tablename, and skip providing the middle parameter is owner.  This can be identified by 2 successive "."
                                if (queryTokens[j + 1].TokenType == TSqlTokenType.Dot && queryTokens[j + 2].TokenType == TSqlTokenType.Dot) //DBName with no owner
                                {
                                    outputdata.DB = sb.ToString();
                                    outputdata.Owner = "Null";
                                    sb.Clear();
                                    //mydb..tablename, the table name in this case is 3 places out from the db name
                                    GetQuotedIdentifier(queryTokens[j + 3], sb);
                                    outputdata.Table = sb.ToString();
                                    sb.Clear();
                                    outputdata.Alias = "Null";
                                    newclause = 0;

                                    //move to next argument
                                    j = j + 4;
                                }
                                //DBName.Owner.TableName
                                if (queryTokens[j + 1].TokenType == TSqlTokenType.Dot && queryTokens[j + 3].TokenType == TSqlTokenType.Dot) //DBName with owner
                                {

                                    outputdata.DB = sb.ToString();
                                    sb.Clear();
                                    GetQuotedIdentifier(queryTokens[j + 2], sb);
                                    outputdata.Owner = sb.ToString();
                                    sb.Clear();
                                    GetQuotedIdentifier(queryTokens[j + 4], sb);
                                    outputdata.Table = sb.ToString();
                                    newclause = 0;
                                    sb.Clear();
                                    outputdata.Alias = "Null";
                                    j = j + 5;
                                }
                                //Owner.TableName
                                if (queryTokens[j + 1].TokenType == TSqlTokenType.Dot && queryTokens[j + 3].TokenType == TSqlTokenType.WhiteSpace) //No DBName with owner
                                {
                                    outputdata.DB = "Null";
                                    outputdata.Owner = sb.ToString();
                                    sb.Clear();
                                    GetQuotedIdentifier(queryTokens[j + 2], sb);
                                    outputdata.Table = sb.ToString();
                                    newclause = 0;
                                    sb.Clear();
                                    outputdata.Alias = "Null";
                                    j = j + 3;
                                }

                                //check for tablename without any prefix.  Need to verify if this is an alias from previouly identified table
                                //by looking at "newclause" variable.  This will be set to 1 if new clause or 0 if appending on as alias
                                if (queryTokens[j - 1].TokenType == TSqlTokenType.WhiteSpace && queryTokens[j + 1].TokenType == TSqlTokenType.WhiteSpace)
                                {
                                    if (newclause == 0) //must be alias name since still part of same From Clause item
                                    {
                                        outputdata.Alias = sb.ToString();
                                        sb.Clear();
                                        j = j + 1;
                                        newclause = 1;
                                    }
                                    else //new item in From clause, must be table name
                                    {
                                        outputdata.DB = "Null";
                                        outputdata.Owner = "Null";
                                        outputdata.Table = sb.ToString();
                                        outputdata.Alias = "Null";
                                        newclause = 0;
                                        sb.Clear();
                                        j = j + 1;
                                    }
                                }

                                // break;
                            }
                        }
                    }

                }
                if (outputdata.DB != null)
                {
                    output.Add(outputdata);
                    outputdata.DB = null;
                    outputdata.Table = null;
                    outputdata.Owner = null;
                    outputdata.Alias = null;
                }
            }
            return output;
        }


        public struct outputstruct
        {
            public string DB;
            public string Owner;
            public string Table;
            public string Alias;
        }
        public struct wherestruct
        {
            public string Table;
            public string Column;
            public string comparison_operator;
            public string comparison_value;
        }
        public struct columnstruct
        {
            public string Alias;
            public string Column;
        }

        private static void GetQuotedIdentifier(TSqlParserToken token, StringBuilder sb)
        {
            switch (token.TokenType)
            {
                case TSqlTokenType.Identifier:
                    sb.Append('[').Append(token.Text).Append(']');
                    break;
                case TSqlTokenType.QuotedIdentifier:
                case TSqlTokenType.Dot:
                    sb.Append(token.Text);
                    break;

                default: throw new ArgumentException("Error: expected TokenType of token should be TSqlTokenType.Dot, TSqlTokenType.Identifier, or TSqlTokenType.QuotedIdentifier");
            }
        }
        private static string getNestedColumn(String column, string direction)
        {
            string output = "";
            bool _continue = true;
            int increment;
            int position;
            if (direction == "reverse")
            {
                position = column.Length-1;
                increment = -1;
            }
            else
            {
                position = 0;
                increment = 1;
            }
                while (_continue && position>=0 &&position < column.Length)
                {
                    if (validatechar((int)column[position])==true)
                    {
                        output = output + column[position];
                        position = position + increment;
                    }
                    else
                    {
                        _continue = false;
                    }
                }
                char[] charArray = output.ToCharArray();
            if (direction == "reverse")
            {
                Array.Reverse(charArray);
            }
                return new string(charArray);
        }
        private static bool validatechar(int ascii)
        {
            QueryContext context = new QueryContext();
            var count = from c in context.columnidentifiers
                        where ascii >= c.asciistart && ascii < c.asciiend
                        select c;
            if (count.Count() > 0)
                return true;
            else
                return false;
        }
        }

    }
