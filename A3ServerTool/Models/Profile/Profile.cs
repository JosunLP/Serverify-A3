﻿namespace A3ServerTool.Models.Profile;

/// <summary>
/// Provides server profile - class-agregator for other configs, startup parameters etc.
/// </summary>
public class Profile : IDataErrorInfo, INotifyPropertyChanged, ICloneable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> class.
    /// </summary>
    public Profile() {}

    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> class.
    /// </summary>
    /// <param name="id">The identifier.</param>
    public Profile(Guid id) : this()
    {
        Id = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Profile"/> class.
    /// </summary>
    /// <param name="argumentSettings">The argument settings.</param>
    /// <param name="id">The identifier.</param>
    [JsonConstructor]
    public Profile(ArgumentSettings argumentSettings, Guid id) : this(id)
    {
        ArgumentSettings = argumentSettings;
    }

    /// <summary>
    /// Gets the profile identifier.
    /// </summary>
    [JsonProperty]
    public Guid Id { get; }

    /// <summary>
    /// Gets or sets the profile name.
    /// </summary>
    public string Name
    {
        get => _name;
        set
        {
            if (Equals(value, _name)) return;
            _name = value;
            OnPropertyChanged();
        }
    }
    private string _name;

    /// <summary>
    /// Gets or sets the profile description.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            if (Equals(value, _description)) return;
            _description = value;
            OnPropertyChanged();
        }
    }
    private string _description;

    /// <summary>
    /// Gets or sets the number of headless clients.
    /// </summary>
    public int? HeadlessClients
    {
        get => _headlessClients;
        set
        {
            if (Equals(value, _headlessClients)) return;
            _headlessClients = value;
            OnPropertyChanged();
        }
    }
    private int? _headlessClients;

    /// <summary>
    /// Gets or sets the path to server executable file.
    /// </summary>
    public string ExecutablePath
    {
        get => _executablePath;
        set
        {
            if (Equals(value, _executablePath)) return;
            _executablePath = value;
            OnPropertyChanged();
        }
    }
    private string _executablePath;

    /// <summary>
    /// Gets or sets the command line argument settings.
    /// </summary>
    public ArgumentSettings ArgumentSettings
    {
        get => _serverSettings;
        set
        {
            if (Equals(value, _serverSettings)) return;
            _serverSettings = value;
            OnPropertyChanged();
        }
    }
    private ArgumentSettings _serverSettings = new ArgumentSettings();

    /// <summary>
    /// Gets or sets the basic network configuration.
    /// </summary>
    [JsonIgnore]
    public BasicConfig BasicConfig
    {
        get => _basicConfig;
        set
        {
            if (Equals(value, _basicConfig)) return;
            _basicConfig = value;
            OnPropertyChanged();
        }
    }
    private BasicConfig _basicConfig = new BasicConfig();

    /// <summary>
    /// Gets or sets the server game configuration.
    /// </summary>
    [JsonIgnore]
    public ServerConfig ServerConfig
    {
        get => _serverConfig;
        set
        {
            if (Equals(value, _serverConfig)) return;
            _serverConfig = value;
            OnPropertyChanged();
        }
    }
    private ServerConfig _serverConfig = new ServerConfig();

    /// <summary>
    /// Gets or sets the arma game profile.
    /// </summary>
    [JsonIgnore]
    public ArmaProfile ArmaProfile
    {
        get => _armaProfile;
        set
        {
            if (Equals(value, _armaProfile)) return;
            _armaProfile = value;
            OnPropertyChanged();
        }
    }
    private ArmaProfile _armaProfile = new ArmaProfile();

    /// <summary>
    /// Gets the path to arma profile.
    /// </summary>
    [JsonIgnore]
    public string ArmaProfilePath => ArmaProfile != null ? ArmaProfile.FileLocation : string.Empty;

    /// <summary>
    /// Gets the path to server configuration file.
    /// </summary>
    [JsonIgnore]
    [ServerArgument(Argument = "-config", IsQuotationMarksRequired = true)]
    public string ServerConfigPath => ServerConfig != null ? ServerConfig.FileLocation : string.Empty;

    /// <summary>
    /// Gets the path to basic configuration file.
    /// </summary>
    [JsonIgnore]
    [ServerArgument(Argument = "-cfg", IsQuotationMarksRequired = true)]
    public string BasicConfigPath => BasicConfig != null ? BasicConfig.FileLocation : string.Empty;

    /// <summary>
    /// Gets the path to profile itself.
    /// </summary>
    [JsonIgnore]
    [ServerArgument(Argument = "-profiles", IsQuotationMarksRequired = true)]
    public string ProfilePath { get; set; }

    #region IDataErrorInfo members

    public string this[string columnName]
    {
        get
        {
            switch (columnName)
            {
                case nameof(Name) when string.IsNullOrWhiteSpace(Name):
                    return "Profile name should be not empty";
                case nameof(Description) when string.IsNullOrWhiteSpace(Description):
                    return "Profile description should be not empty";
                default:
                    return null;
            }
        }
    }

    [JsonIgnore]
    public string Error => string.Empty;

    #endregion

    #region INotifyPropertyChanged members

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion

    #region ICloneable
    public object Clone()
    {
        return this.MemberwiseClone();
    }

    #endregion
}
