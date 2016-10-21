using Akka.Actor;
using System;

namespace AkkaClusterRouter.Actors
{
    public class SomeActor: ReceiveActor
    {
        public SomeActor(object bob)
        {
            Console.WriteLine(bob?.ToString());

            Receive<object>( msg =>{
                Console.WriteLine(msg?.ToString());
            });
        }
    }
}
