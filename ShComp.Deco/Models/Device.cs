namespace ShComp.Deco.Models;

internal class Device
{
    [JsonPropertyName("nand_flash")]
    public bool NandFlash { get; set; }

    [JsonPropertyName("device_ip")]
    public string? DeviceIP { get; set; }

    [JsonPropertyName("owner_transfer")]
    public bool OwnerTransfer { get; set; }

    [JsonPropertyName("bssid_5g")]
    public string? Bssid5g { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("speed_get_support")]
    public bool SpeedGetSupport { get; set; }

    [JsonPropertyName("parent_device_id")]
    public string? ParentDeviceId { get; set; }

    [JsonPropertyName("software_ver")]
    public string? SoftwareVer { get; set; }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("bssid_sta_5g")]
    public string? BssidSta5g { get; set; }

    [JsonPropertyName("device_id")]
    public string? DeviceId { get; set; }

    [JsonPropertyName("product_level")]
    public int ProductLevel { get; set; }

    [JsonPropertyName("bssid_sta_2g")]
    public string? BssidSta2g { get; set; }

    [JsonPropertyName("nickname")]
    public string? Nickname { get; set; }

    [JsonPropertyName("inet_status")]
    public string? InetStatus { get; set; }

    [JsonPropertyName("support_plc")]
    public bool PupportPlc { get; set; }

    [JsonPropertyName("mac")]
    public string? Mac { get; set; }

    [JsonPropertyName("custom_nickname")]
    public string? CustomNickname { get; set; }

    [JsonPropertyName("set_gateway_support")]
    public bool SetGatewaySupport { get; set; }

    [JsonPropertyName("inet_error_msg")]
    public string? InetErrorMsg { get; set; }

    [JsonPropertyName("connection_type")]
    public string[]? ConnectionType { get; set; }

    [JsonPropertyName("hardware_ver")]
    public string? HardwareVer { get; set; }

    [JsonPropertyName("group_status")]
    public string? GroupStatus { get; set; }

    [JsonPropertyName("bssid_2g")]
    public string? Bssid2g { get; set; }

    [JsonPropertyName("oem_id")]
    public string? OemId { get; set; }

    //[JsonPropertyName("signal_level")]
    //public SignalLevel? SignalLevel { get; set; }

    [JsonPropertyName("device_model")]
    public string? DeviceModel { get; set; }

    [JsonPropertyName("oversized_firmware")]
    public bool OversizedFirmware { get; set; }

    [JsonPropertyName("topology")]
    public Topology? Topology { get; set; }

    [JsonPropertyName("hw_id")]
    public string? HWId { get; set; }

    [JsonPropertyName("device_type")]
    public string? DeviceType { get; set; }

    [JsonPropertyName("port_count")]
    public int PortCount { get; set; }

    [JsonPropertyName("zigbee_role")]
    public string? ZigbeeRole { get; set; }

    [JsonPropertyName("enable_gateway_feature")]
    public bool EnableGatewayFeature { get; set; }
}
