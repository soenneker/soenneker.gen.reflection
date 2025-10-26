# Soenneker.Gen.Reflection Benchmarks

This directory contains benchmarking tests that compare the performance of `Soenneker.Gen.Reflection` against `System.Reflection`.

## Available Benchmarks

### 1. TypeInfoAccessBenchmark
Compares getting type information using:
- `GetTypeGen()` (Soenneker.Gen.Reflection)
- `GetType()` (System.Reflection)

### 2. PropertyAccessBenchmark
Compares property access operations:
- Getting individual properties by name
- Getting all properties
- Using both Soenneker.Gen.Reflection and System.Reflection

### 3. FieldAccessBenchmark
Compares field access operations:
- Getting individual fields by name
- Getting all fields
- Getting field values
- Using both Soenneker.Gen.Reflection and System.Reflection

### 4. MethodAccessBenchmark
Compares method access operations:
- Getting individual methods by name
- Getting all methods
- Getting methods with parameters
- Using both Soenneker.Gen.Reflection and System.Reflection

### 5. ComplexTypeBenchmark
Compares complex type operations:
- Nested property access
- Generic type handling
- Nullable type handling
- Generic type arguments
- Using both Soenneker.Gen.Reflection and System.Reflection

### 6. SimpleBenchmarkTest
A simple benchmark for basic functionality testing.

## Running Benchmarks

To run all benchmarks:
```bash
dotnet test --filter "BenchmarkRunner"
```

To run a specific benchmark:
```bash
dotnet test --filter "SimpleBenchmarkTest"
```

## Benchmark Results

The benchmarks will show:
- **Baseline**: Soenneker.Gen.Reflection (marked as baseline = true)
- **Comparison**: System.Reflection performance relative to baseline
- **Memory allocation**: Memory usage comparison
- **Execution time**: Time performance comparison

## Expected Results

Soenneker.Gen.Reflection should generally show:
- **Faster execution**: Compile-time generated code vs runtime reflection
- **Lower memory allocation**: Pre-generated type information vs dynamic reflection
- **Better performance**: Especially for repeated operations

## Notes

- All benchmarks use `[LocalFact]` attributes, so they only run in local development
- Benchmarks are configured with `[MemoryDiagnoser]` and `[SimpleJob]` for consistent results
- Each benchmark includes proper setup with `[GlobalSetup]` methods
