using System;

namespace SuperPostDroidPunk.Models
{
    public class AddDateTime
    {
        private DateTime createAt;
        private DateTime modifiedAt;

        /// <summary>
        /// Save DateTime in database as UTC and return it to the user
        /// in his local format
        /// </summary>
        public AddDateTime()
        {
            ModifiedAt = DateTime.Now;
        }

        public DateTime CreateAt
        {
            get => createAt.ToLocalTime();
            set => createAt = value.ToUniversalTime();
        }

        public DateTime ModifiedAt
        {
            get => modifiedAt.ToLocalTime();
            set => modifiedAt = value.ToUniversalTime();
        }
    }
}
