using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.Metrics;
using Microsoft.Extensions.Hosting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Core;

public interface IConfigureService<T> where T : class {
    void Configure(T service);
}

public static class LinqExtensions
{
    public static void Each<T>(this IEnumerable<T> seq, Action<T> action)
    {
        foreach (var t in seq) action(t);
    }
}    

public class HostedGame(IServiceProvider serviceProvider) : IHostedService {
    private Task _task = Task.CompletedTask;
    private CancellationTokenSource source = new CancellationTokenSource();
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.CanBeCanceled) return Task.CompletedTask;

        return Task.Run(() =>
        {
            var game = new GameEntry(serviceProvider);
            source.Token.Register(o => RunWhenTrue(() => o is Game game, game.Exit), game);
            game.Run();
        });
    }
       
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (cancellationToken.CanBeCanceled) return Task.CompletedTask;
        return source.CancelAsync();
    }
    private static void RunWhenTrue(Func<bool> predicate, Action action) {
        if (predicate()) action();
    }
    private class GameEntry(IServiceProvider serviceProvider) : Game
    {
        private IServiceProvider ServiceProvider => serviceProvider;
        protected override void Initialize()
        {
            ServiceProvider.GetServices<IConfigureService<GameWindow>>()
                .Each(config => config.Configure(Window));
            base.Initialize();
            ServiceProvider.GetServices<IGameComponent>()
                .Each(Components.Add);
        }

        protected override void LoadContent()
        {
            base.LoadContent();
            ServiceProvider.GetServices<IConfigureService<ContentManager>>()
                .Each(config => config.Configure(Content));
        }
    }
}

