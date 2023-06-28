using System.Diagnostics;

namespace PathfindingDemo.Util;

internal class GameTasks
{
    public static GameTasks? Instance { get; private set; }

    public bool IsRunning { get; set; }
    private readonly Thread updateThread;

    /// <summary>
    /// is called before the first frame
    /// </summary>
    public event Action? StartEvent;

    /// <summary>
    /// is called every frame
    /// </summary>
    public event Action? UpdateEvent;


    public GameTasks()
    {
        //make sure that a GameTasks doesn't already exist
        if (Instance != null)
            throw new InvalidOperationException($"an instance of {nameof(GameTasks)} already exists!");

        //initialize GameTasks object
        updateThread = new Thread(() => UpdateLoop()); //create the update thread. Which calls UpdateLoop() via an anonymous function
        IsRunning = false; //set the game to not be running
        Instance = this; //set the instance to this
    }

    /// <summary>
    /// invokes <see cref="StartEvent"/> and then starts the update loop
    /// </summary>
    public void Start()
    {
        if (IsRunning)
            throw new InvalidOperationException($"{nameof(GameTasks)} is already running");

        //set IsRunning to 'true'
        IsRunning = true;

        StartEvent?.Invoke(); //invoke the startEvent if it isn't null
        updateThread.Start(); //start the update thread
    }

    private void UpdateLoop()
    {
        const int DelayMilliseconds = 100; // 1000 ÷ 100 = 10fps
        DateTime startTime;
        DateTime endTime;

        //whilst the game is running
        while (IsRunning)
        {
            startTime = DateTime.Now;
            UpdateEvent?.Invoke();
            endTime = DateTime.Now;

            //calculate the delay that still needs to be waited
            int delay = DelayMilliseconds - (endTime - startTime).Milliseconds;

            //if the delay is greater than 0
            if (delay > 0)
                Thread.Sleep(delay); //wait for the delay

            //if the delay is less than 0
            if (delay < 0)
                Debug.WriteLine($"frame was buffering, took {Math.Abs(delay)}ms longer than the set framerate");
        }
    }
}
