﻿using MyAddressBookPlus.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using StackExchange.Redis;
using Newtonsoft.Json;
using System.Data.SqlClient;
using Microsoft.Azure.Services.AppAuthentication;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Metadata.Edm;
using System.Reflection;

namespace MyAddressBookPlus
{
    public class ContactService
    {
        // Redis cache initialization
        private static Lazy<ConnectionMultiplexer> lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
        {
            string cacheConnection = KeyVaultService.CacheConnection; // ConfigurationManager.AppSettings["CacheConnection"].ToString();
            return ConnectionMultiplexer.Connect(cacheConnection);
        });        
        IDatabase cache = lazyConnection.Value.GetDatabase();

        /// <summary>
        /// Gets all contacts from database
        /// </summary>
        /// <returns></returns>
        public List<Contact> GetContacts()
        {
            try
            {
                var context = new MyAddressBookPlusEntities(new SqlConnection());
                var contacts = context.Contacts.ToList();
                return contacts;
            }
            catch (Exception ex)
            {
                return new List<Contact>
                    {
                        new Contact()
                        {
                            Address = ex.StackTrace,
                            Name = ex.Message                        }
                    };
            }
        }

        /// <summary>
        /// Gets a specific contact from database
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Contact GetContact(int id)
        {
            var context = new MyAddressBookPlusEntities(new SqlConnection());
            var contact = context.Contacts.SingleOrDefault(c => c.Id == id);

            return contact;
        }

        /// <summary>
        /// Gets a specific contact from Redis cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Contact GetContactFromCache(int id)
        {
            var cacheContent = cache.StringGet(id.ToString());
            if (!cacheContent.IsNull)
            {
                var contact = JsonConvert.DeserializeObject<Contact>(cacheContent);
                return contact;
            }

            return null;
        }

        /// <summary>
        /// Adds a new contact to the database and cache
        /// </summary>
        /// <param name="contact"></param>
        /// <returns></returns>
        public int AddContact(Contact contact)
        {
            var context = new MyAddressBookPlusEntities(new SqlConnection());
            context.Contacts.Add(contact);
            context.SaveChanges();

            var newId = contact.Id;

            // add new contact to cache
            cache.StringSet(newId.ToString(), JsonConvert.SerializeObject(contact));

            return newId;
        }

        /// <summary>
        /// deletes a given contact from database and cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteContact(int id)
        {
            var context = new MyAddressBookPlusEntities(new SqlConnection());
            var contactToDelete = context.Contacts.SingleOrDefault(c => c.Id == id);

            if(contactToDelete == null)
            {
                return false;
            }

            context.Contacts.Remove(contactToDelete);
            context.SaveChanges();

            // remove the item from cache
            cache.KeyDelete(id.ToString());

            return true;
        }
    }
}