using DataAccessLibrary.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLibrary
{
    public class SqlCrud
    {
        private readonly string _connectionString;
        private SqlDataAccess db = new SqlDataAccess();
        public SqlCrud(string connectString)
        {
            _connectionString = connectString;
        }

        public List<BasicContactModel> GetAllContacts()
        {
            string sql = "select Id, FirstName, LastName from dbo.Contacts";

           return db.LoadData<BasicContactModel, dynamic>(sql, new { }, _connectionString);
        }

        public FullContactModel GetFullContactById(int id)
        {
            //parameterize seuql statement
            // want to manually build string,no concatct, open to sql injection but 
            // this case it's int or id, so not really cant put other values

            string sql = "select Id, FirstName, LastName from dbo.Contacts where Id = @Id";
            FullContactModel output = new FullContactModel();
            output.BasicInfo = db.LoadData<BasicContactModel, dynamic>(sql, new { Id = id }, _connectionString).FirstOrDefault();
            //return just the first record, if ther'e sno record not throw eception but will return a defualt vlaue which is null.

            if (output.BasicInfo == null)
            {
                // do tell user that the record was not found
                return null;
            }

            sql = @"select  e.*
                from dbo.EmailAddresses e
                inner join dbo.ContactEmail ce on ce.EmailAddressId = e.Id
                where ce.ContactId = @Id";

            output.EmailAddresses = db.LoadData<EmailAddressModel, dynamic>(sql, new { Id = id }, _connectionString);

            sql = @"select  p.*
                from dbo.PhoneNumbers p
                inner join dbo.ContactPhoneNumbers cp on cp.PhoneNumberId = p.Id
                where cp.ContactId = @Id";

            output.PhoneNumber = db.LoadData<PhoneNumberModel, dynamic>(sql, new { Id = id }, _connectionString);

            return output;

        }
    }  
}
 