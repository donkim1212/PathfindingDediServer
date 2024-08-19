using PathfindingDedicatedServer.Src.Constants;
using PathfindingDedicatedServer.Nav;
using PathfindingDedicatedServer.Nav.Crowds;
using PathfindingDedicatedServer.Src.Network;
using PathfindingDedicatedServer.Src.Utils;
using PathfindingDedicatedServer.Src.Utils.FileLoader;
using System.Net;
using System.Net.Sockets;
using PathfindingDedicatedServer.Src.Data;

namespace PathfindingDedicatedServer;
public class Program
{
  public static void Main()
  {
    Init();

    NavManager cm = new (1);
    cm.Start();
    cm.AddMonster(1);
    cm.AddMonster(2);
    Console.WriteLine("pos: " + cm.GetMonsterPos(1));
    Console.WriteLine("pos: " + cm.GetMonsterPos(2));

    SchedulerUtils.SetIntervalAction(1000, () =>
    {
      Console.WriteLine($"[ 1 ] pos: " + cm.GetMonsterPos(1));
      Console.WriteLine($"[ 2 ] pos: " + cm.GetMonsterPos(2));
    });

    // Start the TCP server
    StartTcpServer();
  }

  private static void Init()
  {
    Console.WriteLine("----- INIT START -----");
    DateTime startTime = DateTime.UtcNow;
    // Load all NavMeshes
    NavMeshLoader.LoadAllNavMeshAssets();

    // Initialize SpawnerManager
    SpawnerManager.Init();

    JsonFileLoader loader = new();
    MonsterAgentData monsterData = loader.LoadFileFromAssets<MonsterAgentData>("MonsterAgentInfo.json");
    Storage.MonsterAgentData = monsterData;
    Console.WriteLine($"Loaded {monsterData.Name}, version: {monsterData.Version}");
    Console.WriteLine($"-- {monsterData.Data.First().MonsterModel}");
    PlayerAgentData playerData = loader.LoadFileFromAssets<PlayerAgentData>("PlayerAgentInfo.json");
    Storage.PlayerAgentData = playerData;
    Console.WriteLine($"Loaded {playerData.Name}, version: {playerData.Version}");
    Console.WriteLine($"-- {playerData.Data.First().CharClass}");
    DateTime endTime = DateTime.UtcNow;
    Console.WriteLine();
    Console.WriteLine($"Elapsed time: {(endTime - startTime).TotalSeconds}s");
    Console.WriteLine("----- INIT END -----");
  }

  private static void StartTcpServer()
  {
    // Set up the TCP listener on port 5000
    IPAddress localhost = IPAddress.Parse("127.0.0.1");
    TcpListener tcpListener = new(localhost, 5000);
    tcpListener.Start();
    Console.WriteLine("TCP Server started on port 5000.");

    while (true)
    {
      try
      {
        // Accept a pending client connection
        TcpClient tcpClient = tcpListener.AcceptTcpClient();
        Console.WriteLine("Client connected.");

        // Handle the client connection in a separate thread
        Thread clientThread = new(start: HandleClient)
        {
          IsBackground = true
        };
        clientThread.Start(tcpClient);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
      }
    }
  }

  private static async void HandleClient(object? obj)
  {
    try
    {
      if (obj is not TcpClient tcpClient)
      {
        return;
      }

      TcpClientHandler handler = new (tcpClient);

      handler.OnDataReceived += (data) =>
      {
        Console.WriteLine($"Received: {data}");
      };

      await handler.StartHandlingClientAsync();
    }
    catch (Exception e)
    {
      Console.WriteLine("HandleClient Error:" + e.Message);
    }

  }
}