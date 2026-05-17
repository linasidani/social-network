using System.Linq.Expressions;

namespace SocialNetwork.Tests;

public class TestAsyncQueryProvider<T> : IQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncQueryable<T>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncQueryable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression)!;
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }
}

public class TestAsyncQueryable<T> : IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryable<T> _inner;

    public TestAsyncQueryable(Expression expression)
    {
        _inner = new List<T>().AsQueryable().Provider.CreateQuery<T>(expression);
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(_inner.GetEnumerator());
    }

    public Type ElementType => _inner.ElementType;
    public Expression Expression => _inner.Expression;
    public IQueryProvider Provider => new TestAsyncQueryProvider<T>(_inner.Provider);

    public IEnumerator<T> GetEnumerator()
    {
        return _inner.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return _inner.GetEnumerator();
    }
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public async ValueTask<bool> MoveNextAsync()
    {
        return await Task.FromResult(_inner.MoveNext());
    }

    public async ValueTask DisposeAsync()
    {
        _inner.Dispose();
        await Task.CompletedTask;
    }
}
