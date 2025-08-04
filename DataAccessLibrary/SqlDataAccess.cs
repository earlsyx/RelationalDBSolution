using Dapper;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public class SqlDataAccess
    {
        //Generics 
        //Load Data - read operations, select * from
        public List<T> LoadData<T, U>(string sqlStatement, U parameters, string connectionString)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                //create a connection to the database.
                // using statements - allows to ensure you are always closing out connection properly
                // no matter what, fails sucess crashes , uanhalehaled, always close connection
                //parameters--limiters to our call, id , maybe no limiters, ietherway return set of data.
                List<T> rows = connection.Query<T>(sqlStatement, parameters).ToList();
                return rows;
            }
        }

        //saving data
        public void SaveData<T>(string sqlStatemnt, T parameters, string connectionString)
        {
            using (IDbConnection connection = new SqlConnection(connectionString))
            {
                //write instead of read, query is read, Execute is a write

                connection.Execute(sqlStatemnt, parameters);
            }
        }
    }
}
