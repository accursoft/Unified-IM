namespace UnifiedIM
{
    using System;

    /// <summary>An IM Message.</summary>
    public class Message
    {
        /// <summary>The contact that sent the message</summary>
        public readonly IContact Contact;

        /// <summary>The message</summary>
        public readonly string Text;

        /// <summary>The message time</summary>
        public readonly DateTime Time;

        /// <summary>true if the recipient was online</summary>
        public readonly bool Online;

        internal protected Message(IContact contact, string text, DateTime time, bool online)
        {
            Contact = contact;
            Text = text;
            Online = online;
            Time = time;
        }
    }
}