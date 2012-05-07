
namespace UnifiedIM
{
    public enum ContactStatus { Online, Offline, Unavailable, Custom }

    /// <summary>Interface for contacts.</summary>
    public interface IContact
    {
        /// <summary>Gets the contact's identifier.</summary>
        /// <value>The contact's identifier.</value>
        string Id { get; }

        /// <summary>Gets contact's display name.</summary>
        /// <value>The contact's display name.</value>
        string DisplayName { get; }

        /// <summary>Gets the contact's status.</summary>
        /// <value>The contact's status.</value>
        ContactStatus Status { get; }

        /// <summary>Gets the contact's status text.</summary>
        /// <value>The contact's status text.</value>
        string StatusText { get; }

        /// <summary>Sends a message.</summary>
        /// <param name="message">The message to send.</param>
        void SendMessage(string message);

        /// <summary>Removes this contact from the contact list.</summary>
        void Remove();
    }

    /// <summary>Interface for contacts that can receive typing events.</summary>
    public interface ISendTyping
    {
        /// <summary>Sends a typing event.</summary>
        void SendTyping();
    }

    /// <summary>Interface for contacts that can receive files.</summary>
    public interface ISendFile
    {
        /// <summary>Sends a file.</summary>
        /// <param name="path">Full pathname of the file.</param>
        void SendFile(string path);
    }
}