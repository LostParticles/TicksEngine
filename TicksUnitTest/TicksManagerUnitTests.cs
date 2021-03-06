﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LostParticles.TicksEngine;
using LostParticles.TicksEngine.Manager;

namespace TicksUnitTest
{
    [TestClass]
    public class TicksManagerUnitTests
    {

        [TestMethod]
        public void TestMethod1()
        {
            TickEvent t0 = new TickEvent(0, 4);
            TickEvent t1 = new TickEvent(1, 3);
            TickEvent t2 = new TickEvent(1, 2);
            TickEvent t3 = new TickEvent(1, 1);
            TickEvent t4 = new TickEvent(0, 2);
            TickEvent t5 = new TickEvent(1, 1);
            

            TicksManager tem = new TicksManager(60); // tick every milli second

            tem.AddEvent(t0);
            tem.AddEvent(t1);
            tem.AddEvent(t2);
            tem.AddEvent(t3);
            tem.AddEvent(t4);
            tem.AddEvent(t5);

            // zero test 
            Assert.AreEqual<double>(0, tem.ElapsedBeats);
            Assert.AreEqual(TickEventState.NotStarted, t0.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t1.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t2.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t3.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t4.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t5.CurrentState);

            tem.SendTick(); // send one tick to make sure that certain events ended
            Assert.AreEqual<double>(0.001, tem.ElapsedBeats);
            Assert.AreEqual(TickEventState.Started, t0.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t1.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t2.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t3.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t4.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t5.CurrentState);



            tem.SendRemainingBeat();
            Assert.AreEqual<double>(1, tem.ElapsedBeats);
            Assert.AreEqual(TickEventState.Started, t0.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t1.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t2.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t3.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t4.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t5.CurrentState);

            tem.SendBeat();
            Assert.AreEqual<double>(2, tem.ElapsedBeats);
            Assert.AreEqual(TickEventState.Started, t0.CurrentState);
            Assert.AreEqual(TickEventState.Started, t1.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t2.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t3.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t4.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t5.CurrentState);

            tem.SendBeat();
            Assert.AreEqual<double>(3, tem.ElapsedBeats);
            Assert.AreEqual(TickEventState.Started, t0.CurrentState);
            Assert.AreEqual(TickEventState.Started, t1.CurrentState);
            Assert.AreEqual(TickEventState.Started, t2.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t3.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t4.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t5.CurrentState);

            tem.SendAccurateTicks(200); // send one tick to make sure that certain events ended
            Assert.AreEqual<double>(3.200, tem.ElapsedBeats);
            Assert.AreEqual(TickEventState.Started, t0.CurrentState);
            Assert.AreEqual(TickEventState.Started, t1.CurrentState);
            Assert.AreEqual(TickEventState.Started, t2.CurrentState);
            Assert.AreEqual(TickEventState.Started, t3.CurrentState);
            Assert.AreEqual(TickEventState.Started, t4.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t5.CurrentState);

            tem.SendRemainingBeat();
            Assert.AreEqual<double>(4, tem.ElapsedBeats);
            Assert.AreEqual(TickEventState.Ended, t0.CurrentState);
            Assert.AreEqual(TickEventState.Ended, t1.CurrentState);
            Assert.AreEqual(TickEventState.Ended, t2.CurrentState);
            Assert.AreEqual(TickEventState.Ended, t3.CurrentState); 
            Assert.AreEqual(TickEventState.Started, t4.CurrentState);
            Assert.AreEqual(TickEventState.NotStarted, t5.CurrentState);

            tem.SendTick(); // send one tick to make sure that certain events ended
            Assert.AreEqual<double>(4.001, tem.ElapsedBeats);
            Assert.AreEqual(TickEventState.Ended, t0.CurrentState);
            Assert.AreEqual(TickEventState.Ended, t1.CurrentState);
            Assert.AreEqual(TickEventState.Ended, t2.CurrentState);
            Assert.AreEqual(TickEventState.Ended, t3.CurrentState); 
            Assert.AreEqual(TickEventState.Started, t4.CurrentState);
            Assert.AreEqual(TickEventState.Started, t5.CurrentState);


            tem.SendRemainingBeat();
            Assert.AreEqual<double>(5, tem.ElapsedBeats);
            Assert.AreEqual(TickEventState.Ended, t0.CurrentState);
            Assert.AreEqual(TickEventState.Ended, t1.CurrentState);
            Assert.AreEqual(TickEventState.Ended, t2.CurrentState);
            Assert.AreEqual(TickEventState.Ended, t3.CurrentState); 
            Assert.AreEqual(TickEventState.Ended, t4.CurrentState);
            Assert.AreEqual(TickEventState.Ended, t5.CurrentState);



        }

    }
}
