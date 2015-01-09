# Tick Events #

The library is a portable library modified from an old code for me that was intended to create a midi sequencer.

yep i created the midi sequencer using this code.

Midi depends on messages that was sent using serial ports 

any way

the library contains `TickEvent` class that you should subclass it with your custom one

`TickEvent` has two parameters in its constructor ..

`TickEvent(double holdBeats, double durationBeats)`
 
holdBeats: event will not be executed until it receives this amount of beats.
durationBeats: event execution duration.

so whenever you add more than one event with the same holdBeats  thest events will start execution together.

`AccurateTickEventsManager` class 

Add TickEvent instances to this class and then call the Start method for execution of your planned events.


`EventsPlayer` can hold more than one `AccurateTickEventsManager` to apply the track concept .. because in midi you can have more than one channel that plays notes together.

ENJOY

Ahmed Sadek