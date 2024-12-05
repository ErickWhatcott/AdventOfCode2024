using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace resources;

public class LinkedDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey : notnull{
    internal Dictionary<TKey, LinkedDictionaryNode<TKey, TValue>> Data { get; set; } = [];
    internal LinkedDictionaryNode<TKey, TValue>? Head { get; set; }
    internal LinkedDictionaryNode<TKey, TValue>? Tail { get; set; }

    public int Count => Data.Count;
    
    public TValue this[TKey key] { 
        get => Data[key].Value;
        set {
            if(Data.TryGetValue(key, out var node)) {
                node.Value = value;
            }else{
                Add(key, value);
            }
        }
    }

    public void Add(TKey key, TValue value) {
        if(Data.ContainsKey(key)) throw new Exception("The key is already present in the dictionary.");
        if(Head is null || Tail is null) {
            Head = new(key, value);
            Tail = Head;
            Data.Add(key, Head);
        }else {
            Tail = Tail.Next = new(key, value);
            Data.Add(key, Tail);
        }
    }

    public bool ContainsKey(TKey key) =>
        Data.ContainsKey(key);

    public bool Remove(TKey key) {
        if(Data.TryGetValue(key, out var node)) {
            var (Next, Last) = (node.Next, node.Last);
            if(node.Last is not null)
                node.Last.Next = Next;
            if(node.Next is not null)
                node.Next.Last = Last;
            return Data.Remove(key);
        }

        return false;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) {
        if(Data.TryGetValue(key, out var node)) {
            value = node.Value;
            return true;
        }
        value = default;
        return false;
    }

    public void Add(KeyValuePair<TKey, TValue> item) =>
        Add(item.Key, item.Value);

    public void Clear() {
        Data.Clear();
        Head = null;
        Tail = null;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) =>
        TryGetValue(item.Key, out var node) && (node?.Equals(item.Value) ?? false);

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
        var current = Head;
        while(current is not null){
            yield return current.KeyValuePair;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();
}

public class LinkedDictionaryNode<TKey, TValue>(TKey key, TValue value) {
    public LinkedDictionaryNode<TKey, TValue>? Next { get; internal set; }
    public LinkedDictionaryNode<TKey, TValue>? Last { get; internal set; }
    public TKey Key { get; internal set; } = key;
    public TValue Value { get; internal set; } = value;
    public KeyValuePair<TKey, TValue> KeyValuePair = new(key, value);
}