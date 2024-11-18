namespace CustomerDataEnum.Enum
{
    /// <summary>
    /// Represents the possible statuses for a message or operation.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// Indicates that the message or operation was sent successfully, 
        /// but not yet confirmed as received or processed.
        /// </summary>
		Sent,

        /// <summary>
        /// Indicates that the message or operation failed to send or process.
        /// </summary>
        Failed,

        /// <summary>
        /// Indicates that the message or operation was successfully delivered or completed.
        /// </summary>
        Delivered
    }
}

