using System.Collections;
using System.Collections.Concurrent;

namespace resources;

public class ProfilerTree<TKey, TValue> : IEnumerable<TreeNode<TKey, TValue>> where TKey: notnull {
    private Dictionary<TKey, TreeNode<TKey, TValue>> Branches { get; } = [];

    public IEnumerable<TreeNode<TKey, TValue>> DepthFirst {
        get {
            IEnumerable<TreeNode<TKey, TValue>> r = [];

            Stack<(TreeNode<TKey, TValue> node, bool processed)> stack = [];
            foreach(var (key, node) in Branches)
                stack.Push((node, false));

            while(stack.Count > 0) {
                var (node, processed) = stack.Pop();
                if(processed) {
                    yield return node;
                    continue;
                }

                stack.Push((node, true));
                foreach(var child in node.GetChildren()) {
                    stack.Push((child, false));
                }
            }
        }
    }

    public TreeNode<TKey, TValue> GetNode(TKey key, IEnumerable<TKey>? parents) {
        var enumerator = parents?.GetEnumerator();
        if(!(enumerator?.MoveNext() ?? false))
            return Branches[key];

        TreeNode<TKey, TValue>? current = Branches.GetValueOrDefault(enumerator.Current);
        while(current is not null && enumerator.MoveNext()) {
            current = current.Children.GetValueOrDefault(enumerator.Current);
        }

        if(current is null) throw new KeyNotFoundException("The key was not found in the Tree.");
        return current.Children[key];
    }

    public TreeNode<TKey, TValue> GetOrAddNode(TKey key, TValue default_value, IEnumerable<TKey>? parents) {
        var enumerator = parents?.GetEnumerator();
        if(!(enumerator?.MoveNext() ?? false)){
            return Branches.GetOrAdd(key, new TreeNode<TKey, TValue>(key, default_value, null));
        }

        TreeNode<TKey, TValue> current = Branches.GetOrAdd(enumerator.Current, new TreeNode<TKey, TValue>(enumerator.Current, default_value, null));
        while(enumerator.MoveNext()) {
            current = current._children.GetOrAdd(enumerator.Current, new TreeNode<TKey, TValue>(enumerator.Current, default_value, current));
        }

        return current._children.GetOrAdd(key, new TreeNode<TKey, TValue>(key, default_value, current));
    }

    public TreeNode<TKey, TValue> GetOrAddNodeFillParents(TKey key, Func<TKey, TValue> default_value, IEnumerable<TKey>? parents) {
        var enumerator = parents?.GetEnumerator();
        if(!(enumerator?.MoveNext() ?? false)){
            return Branches.GetOrAdd(key, new TreeNode<TKey, TValue>(key, default_value(key), null));
        }

        TreeNode<TKey, TValue> current = Branches.GetOrAdd(enumerator.Current, new TreeNode<TKey, TValue>(enumerator.Current, default_value(enumerator.Current), null));
        while(enumerator.MoveNext()) {
            current = current._children.GetOrAdd(enumerator.Current, new TreeNode<TKey, TValue>(enumerator.Current, default_value(enumerator.Current), current));
        }

        return current._children.GetOrAdd(key, new TreeNode<TKey, TValue>(key, default_value(key), current));
    }

    public IEnumerator<TreeNode<TKey, TValue>> GetEnumerator() =>
        DepthFirst.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}

public class TreeNode<TKey, TValue>(TKey i, TValue v, TreeNode<TKey, TValue>? p) where TKey : notnull {
    internal readonly Dictionary<TKey, TreeNode<TKey, TValue>> _children = [];

    public TKey Index { get; } = i;
    public TValue Value { get; } = v;
    public TreeNode<TKey, TValue>? Parent { get; } = p;

    public IReadOnlyDictionary<TKey, TreeNode<TKey, TValue>> Children => _children;
    public IEnumerable<TreeNode<TKey, TValue>> Ancestors {
        get{
            var current = Parent;
            while(current is not null) {
                yield return current;
                current = current.Parent;
            }
        }
    }

    public IEnumerable<TreeNode<TKey, TValue>> AncestorsTopDown {
        get{
            Stack<TreeNode<TKey, TValue>> stack = [];
            var current = Parent;
            while(current is not null) {
                stack.Push(current);
                current = current.Parent;
            }
            return stack;
        }
    }

    public IEnumerable<TreeNode<TKey, TValue>> GetChildren() =>
        Children.Values.Reverse();

    public TreeNode<TKey, TValue> AddChild(TKey key, TValue value) {
        TreeNode<TKey, TValue> node = new(key, value, this);
        _children.TryAdd(key, node);
        return node;
    }

    public override string ToString() => 
        Index.ToString() ?? throw new Exception();
}