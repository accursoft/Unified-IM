using System;

using Bricksoft.IM.Yahoo;

namespace UnifiedIM
{
    class YahooContact : IContact, ISendTyping, ISendFile
    {
        readonly YahooBuddy contact;
        readonly YahooClient client;

        internal YahooContact(YahooBuddy contact, YahooClient client)
        {
            this.contact = contact;
            this.client = client;
        }

        public string Id { get { return contact.Account; } }

        public string DisplayName { get { return contact.Account; } }

        public ContactStatus Status
        {
            get
            {
                switch (contact.Status) {
                    case YahooStatus.YAHOO_STATUS_IDLE:
                    case YahooStatus.YAHOO_STATUS_WEBLOGIN:
                    case YahooStatus.YAHOO_STATUS_AVAILABLE: return ContactStatus.Online;
                    case YahooStatus.YAHOO_STATUS_OFFLINE:
                    case YahooStatus.YAHOO_STATUS_INVISIBLE: return ContactStatus.Offline;
                    case YahooStatus.YAHOO_STATUS_NOTATDESK:
                    case YahooStatus.YAHOO_STATUS_NOTATHOME:
                    case YahooStatus.YAHOO_STATUS_BRB:
                    case YahooStatus.YAHOO_STATUS_BUSY:
                    case YahooStatus.YAHOO_STATUS_ONPHONE:
                    case YahooStatus.YAHOO_STATUS_ONVACATION:
                    case YahooStatus.YAHOO_STATUS_OUTTOLUNCH:
                    case YahooStatus.YAHOO_STATUS_STEPPEDOUT:
                    case YahooStatus.YAHOO_STATUS_NOTINOFFICE: return ContactStatus.Unavailable;
                    default: return ContactStatus.Custom;
                }
            }
        }

        public string StatusText
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(contact.StatusMessage)) return contact.StatusMessage;

                return YahooStatus.YAHOOStatusAsString(contact.Status);
            }
        }

        public void SendMessage(string message)
        {
            client.SendMessage(Id, message);
        }

        public void SendTyping()
        {
            client.SendTypingMessage(Id);
        }

        public void SendFile(string path)
        {
            client.SendFile(Id, path);
        }

        public void Remove()
        {
            client.DeleteBuddy(contact);
        }
    }

    /// <summary>Yahoo connection.</summary>
    public class YahooConnection : Connection, IReceiveTyping, ICustomStatus
    {
        YahooClient yahooClient { get { return (YahooClient) Client; } }

        /// <summary>Initializes a new instance of the <see cref="YahooConnection"/> class.</summary>
        /// <param name="account"> The account.</param>
        /// <param name="password">The password.</param>
        public YahooConnection(string account, string password) : base(new YahooClient(), account, password) 
        {
            yahooClient.OnBuddyTyping += (sender, buddy) => { IsTyping(this, new EventArgs<IContact>(_contacts[buddy])); };
        }

        protected override IContact ContactFactory(string buddy)
        {
            return new YahooContact((YahooBuddy) Client.BuddyList[buddy], yahooClient);
        }

        /// <summary>Occurs when a contact is typing.</summary>
        public event EventHandler<EventArgs<IContact>> IsTyping = delegate { };

        /// <summary>Sets the connection's status.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the status is not one of ContactStatus.Online, ContactStatus.Offline, or ContactStatus.Unavailable.</exception>
        /// <param name="status">The connection's status.</param>
        public override void SetStatus(ContactStatus status)
        {
            switch (status) {
                case ContactStatus.Online: yahooClient.Status = YahooStatus.YAHOO_STATUS_AVAILABLE;
                    break;
                case ContactStatus.Offline: yahooClient.Status = YahooStatus.YAHOO_STATUS_OFFLINE;
                    break;
                case ContactStatus.Unavailable: yahooClient.Status = YahooStatus.YAHOO_STATUS_BUSY;
                    break;
                default: throw new InvalidOperationException();
            }
        }

        /// <summary>Sets a custom status.</summary>
        /// <param name="status">The connection's status.</param>
        public void SetCustomStatus(string status)
        {
            yahooClient.ChangeStatus(YahooStatus.YAHOO_STATUS_CUSTOM, status);
        }

        /// <summary>Adds a contact.</summary>
        /// <param name="id">The contact to add.</param>
        public override void AddContact(string id)
        {
            yahooClient.AddBuddy(string.Empty, id, string.Empty);
        }
    }
}