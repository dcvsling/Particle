namespace Particle;

public delegate void RefAction<T>(ref T particle) where T : struct;
public delegate void ReadonlyAction<T>(in T particle) where T : struct;
public delegate TResult ReadonlyFunc<T, TResult>(in T particle) where T : struct;
public delegate bool ReadonlyPredicate<T>(in T particle) where T : struct;
