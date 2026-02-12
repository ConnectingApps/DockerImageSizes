# Docker Image Sizes, Security & Compatibility for .NET

This project demonstrates the trade-offs between **image size**, **security (CVEs)**, and **native library compatibility** when choosing a Docker base image for .NET applications. It compares three popular options: **Debian**, **Alpine**, and **Ubuntu Chiseled**.

---

## The Demo Application

The application is a simple .NET 8 console app that calls a **native glibc library** (`libuuid.so.1`) via P/Invoke to generate a UUID. This deliberate use of a native dependency exposes compatibility differences between base images.

### Program.cs

```csharp
using System;
using System.Runtime.InteropServices;

class Program
{
    [DllImport("libuuid.so.1")]
    private static extern void uuid_generate(byte[] buffer);

    static void Main()
    {
        Console.WriteLine("Generating UUID using native glibc library...");

        try
        {
            var buffer = new byte[16];
            uuid_generate(buffer);
            Console.WriteLine("Success.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception:");
            Console.WriteLine(ex);
        }
    }
}
```

### NativeDemo.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
  </PropertyGroup>
</Project>
```

---

## The Three Dockerfiles

### 1. Debian (full runtime)

Uses the standard .NET runtime image based on Debian and installs `libuuid1` via `apt`.

```dockerfile
# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY NativeDemo.csproj .
RUN dotnet restore
COPY Program.cs .
RUN dotnet publish -c Release -o /app

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/runtime:8.0

RUN apt update && apt install -y libuuid1

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "NativeDemo.dll"]
```

**Result:** ✅ Works — Debian ships glibc, and `libuuid1` is readily available.

---

### 2. Alpine (musl-based)

Uses the Alpine-based .NET runtime image. No native libraries are installed.

```dockerfile
# ===== Build stage =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY NativeDemo.csproj .
RUN dotnet restore
COPY Program.cs .
RUN dotnet publish -c Release -o /app

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine

# Alpine uses musl instead of glibc — no compatible libuuid.so.1 available

WORKDIR /app
COPY --from=build /app .

ENTRYPOINT ["dotnet", "NativeDemo.dll"]
```

**Result:** ❌ Fails with `System.DllNotFoundException` — Alpine uses **musl** instead of **glibc**, so `libuuid.so.1` (a glibc library) cannot be loaded, even if you were to install a uuid package.

---

### 3. Ubuntu Chiseled (minimal glibc-based)

Uses the chiseled (distroless-style) Ubuntu-based .NET runtime image and manually copies `libuuid.so.1` from a full Ubuntu build stage.

```dockerfile
# ===== Build stage (.NET) =====
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY NativeDemo.csproj .
RUN dotnet restore
COPY Program.cs .
RUN dotnet publish -c Release -o /app

# ===== Extract libuuid from Ubuntu =====
FROM ubuntu:22.04 AS uuidstage
RUN apt update && apt install -y libuuid1

# ===== Runtime stage =====
FROM mcr.microsoft.com/dotnet/runtime:8.0-jammy-chiseled

WORKDIR /app
COPY --from=build /app .

COPY --from=uuidstage /usr/lib/x86_64-linux-gnu/libuuid.so.1 /usr/lib/

ENTRYPOINT ["dotnet", "NativeDemo.dll"]
```

**Result:** ✅ Works — Chiseled images are based on Ubuntu (glibc), so the copied `libuuid.so.1` is fully compatible.

---

## Results

### Image Sizes

| Image | Base | Size |
|-------|------|------|
| `demo-debian` | `runtime:8.0` (Debian) | **213 MB** |
| `demo-alpine` | `runtime:8.0-alpine` | **84 MB** |
| `demo-chiseled` | `runtime:8.0-jammy-chiseled` | **85.5 MB** |

### Vulnerability Scans (Trivy)

| Image | Total CVEs | Critical | High | Medium | Low |
|-------|-----------|----------|------|--------|-----|
| `demo-debian` | **87** | 1 | 2 | 24 | 60 |
| `demo-alpine` | **0** | 0 | 0 | 0 | 0 |
| `demo-chiseled` | **3** | 0 | 0 | 0 | 3 |

### Functional Compatibility

| Image | Native glibc P/Invoke | Status |
|-------|----------------------|--------|
| `demo-debian` | `libuuid.so.1` | ✅ Works |
| `demo-alpine` | `libuuid.so.1` | ❌ `DllNotFoundException` |
| `demo-chiseled` | `libuuid.so.1` | ✅ Works |

---

## How to Run

```bash
# Debian
docker build -f Dockerfile.debian -t demo-debian .
docker run demo-debian

# Alpine
docker build -f Dockerfile.alpine -t demo-alpine .
docker run demo-alpine

# Chiseled
docker build -f Dockerfile.chiseled -t demo-chiseled .
docker run demo-chiseled
```

---

## Conclusion

This project proves that **Ubuntu Chiseled images are the best compromise** for .NET applications that depend on native glibc libraries:

1. **Alpine** is the smallest and most secure (0 CVEs), but it **breaks glibc-dependent workloads** because it uses musl. If your application relies on native Linux libraries built against glibc — which is common in the .NET ecosystem — Alpine is simply not an option.

2. **Debian** is the most compatible and easiest to work with, but it comes at a steep cost: **2.5× the image size** and **87 known vulnerabilities**, including a critical one. The full Debian userland ships far more packages than your application needs, expanding both the attack surface and the image footprint.

3. **Ubuntu Chiseled** delivers the best of both worlds:
   - ✅ **glibc-compatible** — native libraries like `libuuid.so.1` work out of the box
   - ✅ **Nearly as small as Alpine** — only 1.5 MB larger (85.5 MB vs. 84 MB)
   - ✅ **Minimal attack surface** — only 3 low-severity CVEs, no shell, no package manager
   
   The only trade-off is that you may need a multi-stage build to copy in specific native libraries (since chiseled images have no package manager), but this is a small price to pay for a production-ready, secure, and compact container.

**When building .NET containers for production, prefer Ubuntu Chiseled images** — especially when your application uses P/Invoke or depends on native glibc libraries.

---

## License

This project is licensed under the [GNU General Public License v3.0](LICENSE).