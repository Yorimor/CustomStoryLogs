namespace CustomStoryLogs;

public class Events
{
    
}

public delegate void MyEventHandler(object source);

public class MyEvent
{
    public event MyEventHandler OnMaximum;
}
