﻿namespace PathfindingDedicatedServer.Src.Constants
{
  internal class ServerConstants
  {
    /* using */
    public static readonly int RANDOM_SEED = DateTime.UtcNow.Millisecond;
    public const int TICK_RATE = 50; // ms

    /* unused */
    public const string VERSION = "1.0.0";
    public const int MAX_CONNECTIONS = 10; // needs testing
  }
}
