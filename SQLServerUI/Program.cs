



using Microsoft.Extensions.Configuration;
using DataAccessLibrary;
using DataAccessLibrary.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        SqlCrud sql = new SqlCrud(GetConnectionString());

        //ReadAllContacts(sql);
        //ReadContact(sql, 1);

        //CreateNewContact(sql);

        // Foreign key relationship
        // not setup, aclled referencial integrity meaning you can delet econtact record without deleting everything that are linked to it. which create orphan records, you really need to have casade delete.best scneario dont delete.

        //UpdateContact(sql);

        RemovePhoneNumberFromContact(sql, 1, 1);
        Console.WriteLine("Done processing");
        Console.ReadLine();
    }

    private static void RemovePhoneNumberFromContact(SqlCrud sql, int contactId, int phoneNumberId)
    {
        sql.RemovePhoneNumberFromContact(contactId, phoneNumberId);
    }
    private static void UpdateContact(SqlCrud sql)
    {
        BasicContactModel contact = new BasicContactModel
        {
            Id = 1,
            FirstName = "Timothy",
            LastName = "Corey"
        };
        sql.UpdateContactName(contact);
    }
    private static void ReadAllContacts(SqlCrud sql)
    {
        var rows = sql.GetAllContacts();

        foreach (var row in rows)
        {
            Console.WriteLine($"{row.Id} :{row.FirstName} {row.LastName}");
        }
    }

    private static void ReadContact(SqlCrud sql, int contactId)
    {
        var contact = sql.GetFullContactById(contactId);
        Console.WriteLine($"{contact.BasicInfo.Id }: {contact.BasicInfo.FirstName} {contact.BasicInfo.LastName}");
        
    }

    private static void CreateNewContact(SqlCrud sql)
    {
        FullContactModel user = new FullContactModel
        {
            BasicInfo = new BasicContactModel
            {
                FirstName = "Charity",
                LastName = "Corey"
            }
        };

        user.EmailAddresses.Add(new EmailAddressModel { EmailAddress = "nope@aol.com" });
        user.EmailAddresses.Add(new EmailAddressModel { Id = 2, EmailAddress = "me@timcorey.com" });

        user.PhoneNumbers.Add(new PhoneNumberModel { Id = 1, PhoneNumber = "555-1212" });
        user.PhoneNumbers.Add(new PhoneNumberModel { PhoneNumber = "555-9876" });

        sql.CreateContact(user);
    }

    private static string GetConnectionString(string connectionStringName = "Default") 
    { 
        string output = "";

        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

        var config = builder.Build();

        output = config.GetConnectionString(connectionStringName);

        return output;
    } 
}