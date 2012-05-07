using System;

using Bricksoft.IM.Jabber;

namespace UnifiedIM
{
    class JabberContact : IContact
    {
        readonly JabberBuddy contact;
        readonly JabberClient client;

        internal JabberContact(JabberBuddy contact, JabberClient client)
        {
            this.contact = contact;
            this.client = client;
        }

        public string Id { get { return contact.Account; } }

        public string DisplayName { get { return contact.Nickname; } }

        public ContactStatus Status
        {
            get
            {
                switch (contact.Status) {
                    case JabberStatus.jsAway: return ContactStatus.Unavailable;
                    case JabberStatus.jsInvisible:
                    case JabberStatus.jsOffline: return ContactStatus.Offline;
                    case JabberStatus.jsOnline: return ContactStatus.Online;
                    default: return ContactStatus.Custom;
                }
            }
        }

        public string StatusText
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(contact.StatusMessage)) return contact.StatusMessage;

                switch (contact.Status) {
                    case JabberStatus.jsAway: return "Away";
                    case JabberStatus.jsCustom: return string.Empty;
                    case JabberStatus.jsInvisible: return "Invisible";
                    case JabberStatus.jsOffline: return "Offline";
                    case JabberStatus.jsOnline: return "Online";
                    default: return string.Empty;
                }
            }
        }

        public void SendMessage(string message)
        {
            client.SendMessage(Id, message);
        }

        public void Remove()
        {
            client.DeleteBuddy(Id);
        }
    }

    /// <summary>Jabber connection. </summary>
    public class JabberConnection : Connection, ICustomStatus
    {
        JabberClient jabberClient { get { return (JabberClient) Client; } }

        /// <summary>Initializes a new instance of the <see cref="JabberConnection"/> class.</summary>
        /// <param name="account">       The account.</param>
        /// <param name="password">      The password.</param>
        /// <param name="server">        The server.</param>
        /// <param name="connectServer"> The connect server.</param>
        /// <param name="authentication">The authentication.</param>
        public JabberConnection(string account, string password, string server, string connectServer, JabberAuthMethod authentication)
            : base(new JabberClient(), account, password)
        {
            jabberClient.Server = server;
            jabberClient.ConnectServer = connectServer;
            jabberClient.ClientAuthMetod = authentication;
            jabberClient.ServerPort = 5222;
        }

        protected override IContact ContactFactory(string buddy)
        {
            return new JabberContact((JabberBuddy) Client.BuddyList[buddy], jabberClient);
        }

        /// <summary>Sets the connection's status.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the status is not one of ContactStatus.Online, ContactStatus.Offline, or ContactStatus.Unavailable.</exception>
        /// <param name="status">The connection's status.</param>
        public override void SetStatus(ContactStatus status)
        {
            switch (status) {
                case ContactStatus.Online: jabberClient.Status = JabberStatus.jsOnline;
                    break;
                case ContactStatus.Offline: jabberClient.Status = JabberStatus.jsOffline;
                    break;
                case ContactStatus.Unavailable: jabberClient.Status = JabberStatus.jsAway;
                    break;
                default: throw new InvalidOperationException();
            }
        }

        /// <summary>Sets a custom status.</summary>
        /// <param name="status">The connection's status.</param>
        public void SetCustomStatus(string status)
        {
            jabberClient.StatusMessage = status;
            jabberClient.Status = JabberStatus.jsCustom;
        }

        /// <summary>Adds a contact.</summary>
        /// <param name="id">The contact to add.</param>
        public override void AddContact(string id)
        {
            jabberClient.AddBuddy(id);
        }
    }
}