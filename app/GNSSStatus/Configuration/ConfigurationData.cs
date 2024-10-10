﻿namespace GNSSStatus.Configuration;

public class ConfigurationData
{
    // Connection
    public required string ServerAddress { get; set; }
    public required int ServerPort { get; set; }
    
    // Coordinate System
    public required int GkSystemNumber { get; set; }
    
    // Static rover antenna base point
    public required double BaseLocationX { get; set; }
    public required double BaseLocationY { get; set; }
    public required double BaseLocationZ { get; set; }
    
    // MQTT
    public required string MqttBrokerAddress { get; set; }
    public required int MqttBrokerPort { get; set; }
    public required string MqttBrokerChannelAltitude { get; set; }
    public required string MqttUsername { get; set; }
    public required string MqttPassword { get; set; }
    public required string MqttClientId { get; set; }
    public required bool UseTls { get; set; }
}