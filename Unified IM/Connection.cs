using System;
using System.Collections.Generic;

using Bricksoft.IM;

namespace UnifiedIM
{
    /// <summary>Base class for IM connections.</summary>
    public abstract class Connection
    {
        protected readonly IMBase Client;

        protected Connection(IMBase client, string account, string password)
        {
            client.Account = account;
            client.Password = password;

            client.OnMessage += (sender, buddy, message) => { MessageReceived(this, new EventArgs<Message>(new Message(_contacts[buddy], message, DateTime.Now, true))); };
            client.OnOfflineMessage += (sender, buddy, message, receivedDateTime) => { MessageReceived(this, new EventArgs<Message>(new Message(_contacts[buddy], message, receivedDateTime, false))); };

            client.OnError += (sender, error) => { Error(this, new EventArgs<string>(error)); };
            client.OnLog += (sender, message) => { Log(this, new EventArgs<string>(message)); };

            client.OnBuddyStatusChanged += (sender, buddy) => { ContactStatusChanged(this, new EventArgs<IContact>(_contacts[buddy])); };

            client.OnReceiveBuddyList += delegate {
                _contacts.Clear();

                foreach (var buddy in client.BuddyList.Keys)
                    _contacts.Add((string) buddy, ContactFactory((string) buddy));

                ContactListReceived(this, new EventArgs<IEnumerable<IContact>>(Contacts));
            };

            client.OnAddBuddy += (sender, buddy) => {
                IContact contact = ContactFactory(buddy);
                _contacts.Add(buddy, contact);
                ContactAdded(this, new EventArgs<IContact>(contact));
            };

            client.OnDeleteBuddy += (sender, buddy) => {
                ContactRemoved(this, new EventArgs<IContact>(_contacts[buddy]));
                _contacts.Remove(buddy);
            };

            Client = client;
        }

        /// <summary>Adds a contact.</summary>
        /// <param name="id">The contact to add.</param>
        public abstract void AddContact(string id);

        /// <summary>Occurs when a contact is added.</summary>
        public event EventHandler<EventArgs<IContact>> ContactAdded = delegate { };

        /// <summary>Occurs when a contact is removed.</summary>
        public event EventHandler<EventArgs<IContact>> ContactRemoved = delegate { };

        /// <summary>Occurs when a contact status changed.</summary>
        public event EventHandler<EventArgs<IContact>> ContactStatusChanged = delegate { };

        /// <summary>Occurs when a contact list is received.</summary>
        public event EventHandler<EventArgs<IEnumerable<IContact>>> ContactListReceived = delegate { };

        protected readonly Dictionary<string, IContact> _contacts = new Dictionary<string, IContact>();
        /// <summary>Gets all contacts.</summary>
        /// <value>All contacts across all connections.</value>
        public IEnumerable<IContact> Contacts { get { return _contacts.Values; } }

        protected abstract IContact ContactFactory(string buddy);

        /// <summary>Gets a value indicating whether the client is connected.</summary>
        /// <value>true if connected, false if not.</value>
        public bool Connected { get { return Client.Logined; } }

        /// <summary>Log on.</summary>
        public void LogOn()
        {
            Client.Login();
        }

        /// <summary>Log off.</summary>
        public void LogOff()
        {
            Client.Logout();
        }

        /// <summary>Occurs when a message is received.</summary>
        public event EventHandler<EventArgs<Message>> MessageReceived = delegate { };

        /// <summary>Event stream for errors.</summary>
        public event EventHandler<EventArgs<string>> Error = delegate { };

        /// <summary>Event stream for log entries.</summary>
        public event EventHandler<EventArgs<string>> Log = delegate { };

        /// <summary>Sets the connection's status.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the status is not one of ContactStatus.Online, ContactStatus.Offline, or ContactStatus.Unavailable.</exception>
        /// <param name="status">The connection's status.</param>
        abstract public void SetStatus(ContactStatus status);
    }
    
    /// <summary>Connections that can set a custom status.</summary>
    public interface ICustomStatus
    {
        /// <summary>Sets a custom status.</summary>
        /// <param name="status">The connection's status.</param>
        void SetCustomStatus(string status);
    }

    /// <summary>Connections that can receive typing events.</summary>
    public interface IReceiveTyping
    {
        /// <summary>Occurs when a contact is typing.</summary>
        event EventHandler<EventArgs<IContact>> IsTyping;
    }
}