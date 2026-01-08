using Godot;
using System;
using System.Collections.Generic;

public partial class ServiceLocator : Node
{
    private static ServiceLocator _instance;
    private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public override void _EnterTree()
    {
        if(_instance != null && _instance != this)
        {
            QueueFree();
            GD.PrintErr("ServiceLocator: try to create second instance. The older one will be used");
            return;
        }
        _instance = this;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            if(_instance == this)
            {
                _instance = null;
                _services.Clear();
                GD.Print("ServiceLocator: instance is deleted and cleared.");
            }
        }
    }

    public static void RegisterService<T>(T service) where T : class
    {
        if (_instance == null)
        {
            GD.PrintErr("ServiceLocator: instance is null. Service not registered.");
            return;
        }
        var type = typeof(T);
        if (!_instance._services.ContainsKey(type))
        {
            _instance._services[type] = service;
            GD.Print($"ServiceLocator: service of type {type.Name} is registered.");
        }
        else 
        {
            GD.Print($"ServiceLocator: Service of type {type.Name} is already registered. Overwriting.");
            _instance._services[type] = service;
        }
    }

    public static T GetService<T>() where T : class
    {
        if (_instance == null)
        {
            GD.PrintErr("ServiceLocator: instance is null. Service not found.");
            return null;
        }
        var type = typeof(T);
        if (_instance._services.TryGetValue(type, out var service))
        {
            return service as T;
        }
        GD.PrintErr($"ServiceLocator: Service of type {type.Name} not found.");
        return null;
    }
}
