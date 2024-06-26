﻿using Akka.Actor;
using Akka.Cluster;
using Akka.Cluster.Tools.Singleton;
using Akka.Configuration;
using Akka.DependencyInjection;
using Asteroids.API.Actors;
using DotNetty.Transport.Bootstrapping;

namespace Asteroids.API.Services;

public class ClusterAkkaService : IHostedService, IActorBridge
{
    private ActorSystem actorSystem;
    private readonly IServiceProvider serviceProvider;
    private readonly IConfiguration configuration;
    private readonly IHostApplicationLifetime appLifeTime;
    private IActorRef lobbySupervisorProxyRef;
    private IActorRef signalRProxyRef;

    public ClusterAkkaService(IServiceProvider serviceProvider, IHostApplicationLifetime appLifeTime, IConfiguration configuration)
    {
        this.serviceProvider = serviceProvider;
        this.appLifeTime = appLifeTime;
        this.configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var actorSystemName = Environment.GetEnvironmentVariable("ACTORSYSTEM");
        var clusterPort = int.Parse(Environment.GetEnvironmentVariable("CLUSTER_PORT"));
        var clusterIp = Environment.GetEnvironmentVariable("CLUSTER_IP");
        var clusterSeed = Environment.GetEnvironmentVariable("CLUSTER_SEED");
        var clusterRoles = Environment.GetEnvironmentVariable("CLUSTER_ROLES");

        var seedNode = $"\"akka.tcp://{actorSystemName}@{clusterSeed}\"";

        var roles = clusterRoles?.Split(',').Select(role => $"\"{role.Trim()}\"").ToList();
        var rolesString = string.Join(", ", roles);

        var config = ConfigurationFactory.ParseString(@$"
            akka {{
                actor {{
                    provider = cluster
                }}
                remote {{
                    dot-netty.tcp {{
                        hostname = {clusterIp}
                        port = {clusterPort}
                    }}
                }}
                cluster {{
                    seed-nodes = [{seedNode}]
                    roles = [{rolesString}]
                    provider = cluster
                }}
            }}
        ");

        var bootstrap = BootstrapSetup.Create().WithConfig(config);
        var diSetup = DependencyResolverSetup.Create(serviceProvider);
        var actorsystemSetup = bootstrap.And(diSetup);

        actorSystem = ActorSystem.Create("asteroids-actor-system", actorsystemSetup);
        var cluster = Cluster.Get(actorSystem);

        var selfAddress = cluster.SelfAddress;
        Cluster.Get(actorSystem).Join(selfAddress);

        if (cluster.SelfRoles.Contains("lobby"))
        {
            var lobbySupervisorProps = DependencyResolver.For(actorSystem).Props<LobbySupervisorActor>();
            var singletonProps = ClusterSingletonManager.Props(
                singletonProps: lobbySupervisorProps,
                terminationMessage: PoisonPill.Instance,
                settings: ClusterSingletonManagerSettings.Create(actorSystem).WithRole("lobby"));
            var lobbiesInstance = actorSystem.ActorOf(singletonProps, "lobbiesSingletonManager");

            var lobbySupervisorProxyProps = ClusterSingletonProxy.Props(
                singletonManagerPath: "/user/lobbiesSingletonManager",
                settings: ClusterSingletonProxySettings.Create(actorSystem));
            lobbySupervisorProxyRef = actorSystem.ActorOf(lobbySupervisorProxyProps, "lobbySupervisorProxy");          
        }

        if (cluster.SelfRoles.Contains("signalR"))
        {
            var signalRHub = Environment.GetEnvironmentVariable("SIGNALRHUB");

            var siganlRActorProps = Props.Create(() => new SignalRActor(lobbySupervisorProxyRef, signalRHub));
            var signalRActor = actorSystem.ActorOf(siganlRActorProps, "siganlRActor");

            var signalRProxyProps = ClusterSingletonProxy.Props(
                singletonManagerPath: "/user/signalRSingleton",
                settings: ClusterSingletonProxySettings.Create(actorSystem));
            signalRProxyRef = actorSystem.ActorOf(signalRProxyProps, "signalRProxy");            
        }


#pragma warning disable CS4014
        actorSystem.WhenTerminated.ContinueWith(_ =>
        {
            appLifeTime.StopApplication();
        });
#pragma warning restore CS4014

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await CoordinatedShutdown.Get(actorSystem).Run(CoordinatedShutdown.ClrExitReason.Instance);
    }

    public void Tell(IActorRef actorRef, object message)
    {
        actorRef.Tell(message);
    }

    public Task<T> Ask<T>(IActorRef actorRef, object message)
    {
        return actorRef.Ask<T>(message);
    }
}
