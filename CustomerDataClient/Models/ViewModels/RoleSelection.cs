namespace CustomerDataClient.Models.ViewModels
{
    /// <summary>
    /// Represents a role that can be selected or deselected, typically used in user role management scenarios.
    /// </summary>
    public class RoleSelection
    {
        /// <summary>
        /// Gets or sets the name of the role.
        /// This should correspond to a role name, such as "Admin" or "User", "etc".
        /// The default value is an empty string to prevent null values.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the role is currently selected.
        /// This is often used to manage which roles are assigned to a user.
        /// </summary>
        public bool IsSelected { get; set; }
    }
}

