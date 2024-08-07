using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Graph
{
    public interface IGraph<T>
    {
        IObservable<IEnumerable<T>> RoutesBetween(T source, T target);
    }

    public class Graph<T> : IGraph<T>
    {
        private readonly Dictionary<T, List<T>> _graph = new Dictionary<T, List<T>>();
        
        public Graph(IEnumerable<ILink<T>> links)
        {
            foreach (var link in links)
            {
                var doesGraphContainSource = _graph.ContainsKey(link.Source);
                var doesGraphContainTarget = _graph.ContainsKey(link.Target);
                
                if (!doesGraphContainSource)
                    _graph[link.Source] = new List<T>();
                
                _graph[link.Source].Add(link.Target);

                // Ensure every node is included in the graph
                if (!doesGraphContainTarget)
                    _graph[link.Target] = new List<T>();
            }
        }

        public IObservable<IEnumerable<T>> RoutesBetween(T source, T target)
        {
            return Observable.Create<IEnumerable<T>>(observer =>
            {
                var paths = new List<List<T>>();
                var visitedNodes = new HashSet<T>();
                
                DepthFirstSearch(source, target, visitedNodes, new List<T>(), paths);
                
                foreach (var path in paths)
                    observer.OnNext(path);
                
                observer.OnCompleted();
                return () => { /* No cleanup required *// };
            });
        }

        private void DepthFirstSearch(T currentNode, T endNode, HashSet<T> visitedNodes, List<T> path, List<List<T>> paths)
        {
            path.Add(currentNode);

            // Check if we've reached the end node//
            if (EqualityComparer<T>.Default.Equals(currentNode, endNode))
            {
                paths.Add(new List<T>(path));
                path.RemoveAt(path.Count - 1);
                return;
            }

            // Mark the current node as visited//
            visitedNodes.Add(currentNode);

            // Explore all neighbors//
            if (_graph.TryGetValue(currentNode, out var neighbors))
            {
                foreach (var neighbor in neighbors.Where(neighbor => !visitedNodes.Contains(neighbor)))
                    DepthFirstSearch(neighbor, endNode, visitedNodes, path, paths);
            }

            // Remove the current node from the visited hashset and remove it from the path
            visitedNodes.Remove(currentNode);
            path.RemoveAt(path.Count - 1);
        }
    }
}