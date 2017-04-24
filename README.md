# ElmahLogger

[![AppVeyour Build](https://ci.appveyor.com/api/projects/status/nq7b4h95ucugasoc?svg=true)](https://ci.appveyor.com/project/knopa/elmah-logger)

ElmahLogger is a free logging platform for .NET for extending Elmah

Installation
-------------

ElmahLogger is available as a NuGet package. You can install it using the NuGet Package Console window:

```
PM> Install-Package ElmahLogger
```

## License
---
ElmahLogger is open source software, licensed under the terms of MIT license.
See [LICENSE.txt](LICENSE.txt) for details.

## Creating Log messages

In order to create log messages from the application you need to use the logging API. There are two classes that you will be using the most: Logger and LogManager, both in the ElmahLogger namespace. Logger represents the named source of logs and has methods to emit log messages, and LogManager creates and manages instances of loggers.

## Creating loggers

It is advised to create one (private static) Logger per class. As mentioned before, you must use LogManager to create Logger instances.

This will create a Logger instance with the same name of the class.
```cs
namespace MyNamespace
{
  public class MyClass
  {
    private static Logger logger = LogManager.GetCurrentClassLogger();
  }
}
```
It's also possible to control the Logger's name:

```cs
using ElmahLogger;

Logger logger = LogManager.GetLogger("MyClassName");
```

Because loggers are thread-safe, you can simply create the logger once and store it in a static variable.

## Writing log messages

In order to emit log message you can simply call one of the methods on the Logger. Logger class has six methods whose names correspond to log levels: Trace(), Debug(), Info(), Warn(), Error() and Fatal(). There is also Log() method which takes log level as a parameter.

```cs
using ElmahLogger;

public class MyClass
{
  private static Logger logger = LogManager.GetCurrentClassLogger();

  public void MyMethod1()
  {
    logger.Log("Sample message");
    logger.Log("Sample message", "Event.Name");
    logger.Log(new Exception("Mesage");
    logger.Log("Sample message", new Exception("Mesage");
    logger.Log("Sample message", new Exception("Mesage"), "Event.Name");
  }
}
```