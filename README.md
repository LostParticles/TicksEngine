# Ticks Engine #

The library is a C# portable library modified ported from an old code of mine that was intended to create a midi sequencer.

yep i created the midi sequencer using this code.

Midi depends on messages that is being sent over serial ports and the mid file actually contains the binary representation of these messages.

any way

the library contains `TickEvent` class that you should subclass it with your custom one

`TickEvent` has two parameters in its constructor ..

`TickEvent(double holdBeats, double durationBeats)`
 
`holdBeats`: event will not be executed until it receives this amount of beats.

`durationBeats`: event execution duration.

The tick events are sequential like the following image.
![](https://github.com/LostParticles/TicksEngine/Documentation/TickEvents Plannging.png)

and the ones with holdBeats = 0 will start at the same time.

## Ticks Manager ##
`Ticks Manager` Class is responsible about tick events start and stop .. it holds the neccessary code through its various functions i.e. `SendTick`, `SendBeat`, and others to control them. 

## Ticks Players ##
`ITicksPlayer`  interface contains the simple functions of Start, Stop, Pause and Players should implement this interface (quite easy).

There are three ticks players included in the library:
 
1- `WindowsTicksPlayer`: Depends windows timer (which in turn is not accurate for multi media scenarios)

2- `HardwareTicksPlayer`: Depends on the hardware timers (`StopWatch`) that I am using it for playing midi files.

3- `MultiPlayer`: This class can hold many instances that implements `ITicksPlayer` so that it sends ticks to them at the same time (I am using it with `HardwareTicksPlayer` for playing back midi files)  


at last .. you can check its usage on my another repository [https://github.com/LostParticles/MusicFramework](https://github.com/LostParticles/MusicFramework "Music Framework")


ENJOY

Ahmed Sadek