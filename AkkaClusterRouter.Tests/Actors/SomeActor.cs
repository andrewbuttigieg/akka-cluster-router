using Akka.Actor;
using System;

namespace AkkaClusterRouter.Actors
{
    public class SomeActor: ReceiveActor
    {
        IActorRef proxyActor { get; }
        public SomeActor(object bob)
        {
            Console.WriteLine(bob?.ToString());

            if (bob is IActorRef)
                proxyActor = bob as IActorRef;

            Receive<object>( msg =>{
                Console.WriteLine(msg?.ToString());
                if (proxyActor != null)
                    proxyActor.Tell(msg);
            });
        }
    }
}
