## Debian Run

```bash
docker build -f Dockerfile.debian -t demo-debian .
```

```bash
docker run demo-debian
```

Output:
```txt
Generating UUID using native glibc library...
Success.
```

## Alpine Run

```bash
docker build -f Dockerfile.alpine -t demo-alpine .
```

```bash
docker run demo-alpine
```

Output:

```
System.DllNotFoundException: Unable to load shared library 'libuuid.so.1' or one of its dependencies. In order to help diagnose loading problems, consider using a tool like strace. If you're using glibc, consider setting the LD_DEBUG environment variable: 
Error loading shared library /usr/share/dotnet/shared/Microsoft.NETCore.App/8.0.24/libuuid.so.1: No such file or directory
Error loading shared library /app/libuuid.so.1: No such file or directory
Error loading shared library libuuid.so.1: No such file or directory
Error loading shared library /usr/share/dotnet/shared/Microsoft.NETCore.App/8.0.24/liblibuuid.so.1: No such file or directory
Error loading shared library /app/liblibuuid.so.1: No such file or directory
Error loading shared library liblibuuid.so.1: No such file or directory
Error loading shared library /usr/share/dotnet/shared/Microsoft.NETCore.App/8.0.24/libuuid.so.1.so: No such file or directory
Error loading shared library /app/libuuid.so.1.so: No such file or directory
Error loading shared library libuuid.so.1.so: No such file or directory
Error loading shared library /usr/share/dotnet/shared/Microsoft.NETCore.App/8.0.24/liblibuuid.so.1.so: No such file or directory
Error loading shared library /app/liblibuuid.so.1.so: No such file or directory
Error loading shared library liblibuuid.so.1.so: No such file or directory
```

## Chiseled

```bash
  docker build -f Dockerfile.chiseled -t demo-chiseled .
```

```bash
docker run demo-chiseled
```

Output:
```txt
Generating UUID using native glibc library...
Success.
```

## Image Sizes

```bash
docker images demo-chiseled
```

```txt
REPOSITORY      TAG       IMAGE ID       CREATED          SIZE
demo-chiseled   latest    3c8bb8daa3cc   30 minutes ago   85.5MB
```


```bash
docker images demo-alpine
```

```txt
REPOSITORY    TAG       IMAGE ID       CREATED          SIZE
demo-alpine   latest    1bd941459fa1   40 minutes ago   84MB
```

```bash
docker images demo-debian
```

```txt
REPOSITORY    TAG       IMAGE ID       CREATED          SIZE
demo-debian   latest    68a05b20ab1a   46 minutes ago   213MB
```

### Vulerability scans

```bash
trivy image demo-alpine
```

```
Total: 0 
```


```bash
trivy image demo-debian
```

Output:
```txt
Total: 87 (UNKNOWN: 0, LOW: 60, MEDIUM: 24, HIGH: 2, CRITICAL: 1)
```


```bash
trivy image demo-chiseled
```

```txt
Total: 3 (UNKNOWN: 0, LOW: 3, MEDIUM: 0, HIGH: 0, CRITICAL: 0)
```