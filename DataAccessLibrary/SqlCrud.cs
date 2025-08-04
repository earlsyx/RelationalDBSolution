using DataAccessLibrary.Models;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

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

            output.PhoneNumbers = db.LoadData<PhoneNumberModel, dynamic>(sql, new { Id = id }, _connectionString);

            return output;

        }

        public void CreateContact(FullContactModel contact)
        {
            //downside of rdms , relational databases
            // takes a bit of work to rehydrate amodel, to dehyrate
            //to break it a part after you have it in c#.

            // Save the basic contact
            // infer , passing  

            string sql = "insert into dbo.Contacts (FirstName, LastName) values (@FirstName, @LastName);";

            db.SaveData(sql,
                        new { contact.BasicInfo.FirstName, contact.BasicInfo.LastName },
                        _connectionString);
            // get the id number of the contact
            sql = "select Id from dbo.Contacts where FirstName = @FirstName and LastName = @LastName; ";

            int contactId = db.LoadData<IdLookUpModel, dynamic>(sql, new { contact.BasicInfo.FirstName, contact.BasicInfo.LastName },_connectionString).First().Id;

            //look up
            // identify if phone number exists

            foreach (var phoneNumber in contact.PhoneNumbers)
            {
                if (phoneNumber.Id == 0)
                {
                    sql = "insert into dbo.PhoneNumbers (PhoneNumber) values (@PhoneNumber);";
                    db.SaveData(sql, new { phoneNumber.PhoneNumber }, _connectionString);

                    sql = "select Id from dbo.PhoneNumbers where PhoneNumber = @PhoneNumber;";
                    phoneNumber.Id = db.LoadData<IdLookUpModel, dynamic>(sql, new { phoneNumber.PhoneNumber }, _connectionString).First().Id;         
                }

                sql = "insert into dbo.ContactPhoneNumbers (ContactId, PhoneNumberId) values (@ContactId, @PhoneNumberId);";

                db.SaveData(sql, new { ContactId = contactId,  PhoneNumberId = phoneNumber.Id }, _connectionString);
            }
            // insert into the link tbale for that number
            // insert the new phone number if not, and get the id
            // then do the link table insert

            // do the same for email.

            foreach (var email in contact.EmailAddresses)
            {
                if (email.Id == 0)
                {
                    sql = "insert into dbo.EmailAddresses (EmailAddress) values (@EmailAddress)";
                    db.SaveData(sql, new { email.EmailAddress }, _connectionString);

                    sql = "select Id from dbo.EmailAddresses where EmailAddress = @EmailAddress;";
                    email.Id = db.LoadData<IdLookUpModel, dynamic>(sql, new { email.EmailAddress }, _connectionString).First().Id;
                }


                sql = "insert into dbo.ContactEmail (ContactId, EmailAddressId) values (@ContactId, @EmailAddressId);";
                db.SaveData(sql, new { ContactId = contactId, EmailAddressId = email.Id }, _connectionString);
            }

        }

        public void UpdateContactName(BasicContactModel contact)
        {
            string sql = "update dbo.Contacts set FirstName = @FirstName, LastName = @LastName where Id = @Id";
            db.SaveData(sql, contact, _connectionString);

        }

        public void RemovePhoneNumberFromContact(int contactId, int phoneNumberId)
        {
            //archive instead of delete

            //Find all of the usages of the phonoe number id
            string sql = "select Id, ContactId, PhoneNumberId from dbo.ContactPhoneNumbers where PhoneNumberId = @PhoneNumberId;";
            var links = db.LoadData<ContactPhoneNumberModel, dynamic>(sql, new { PhoneNumberId = phoneNumberId }, _connectionString);
            // If 1, then delete link and phone number
            sql = "delete from dbo.ContactPhoneNumbers where PhoneNumberId = @PhoneNumberId and ContactId = @ContactId";
            db.SaveData(sql, new { PhoneNumberId = phoneNumberId, ContactId = contactId }, _connectionString);
            if (links.Count == 1)
            {
                sql = "delete from dbo.PhoneNumbers where Id = @PhoneNumberId;";
                db.SaveData(sql, new { PhoneNumberId = phoneNumberId }, _connectionString);
            }
             // if > 1, then delete link for contact
            
        }
    }   
}
 