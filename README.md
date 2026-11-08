# Sistema de matrícula universitaria

Esta es la aplicación ASP.NET Core MVC del avance 2. Toma el prototipo del avance 1 y le agrega autenticación real, persistencia y las historias HU01, HU02, HU09, HU10, HU11 y HU12.

## Cómo está armada en esta máquina

El proyecto usa `net8.0` y el SDK 8.0.425, fijado en `global.json`. Hay un SDK 11 instalado, pero es una versión preliminar y no se usó.

La base está en SQL Server Express, en `localhost\SQLEXPRESS`. LocalDB no está instalado. La conexión entra con autenticación de Windows, así que `appsettings.json` no guarda ninguna contraseña.

Las contraseñas del administrador y de las cuentas de demostración salen de user-secrets o de variables de entorno. No están en el repositorio.

## Puesta en marcha

Desde esta carpeta, con el SDK que indica `global.json`:

```text
dotnet tool install --global dotnet-ef --version 8.0.11
dotnet ef database update --project ProyectoFinal_PorrasMarco
dotnet user-secrets --project ProyectoFinal_PorrasMarco set "Seed:AdminPassword" "<definir>"
dotnet user-secrets --project ProyectoFinal_PorrasMarco set "Seed:ContrasenaDemostracion" "<definir>"
dotnet run --project ProyectoFinal_PorrasMarco --launch-profile http
```

Al iniciar, la aplicación aplica las migraciones, crea los roles y carga el catálogo de demostración. Si faltan esas dos contraseñas, el catálogo se carga, pero las cuentas de ejemplo no. El registro público, en `/registro`, crea solo estudiantes.

Si SQL Server no es Express, hay que cambiar `ConnectionStrings:DefaultConnection` en `appsettings.json`.

## Cuentas de demostración

Los correos son sintéticos. Las contraseñas quedan solo en la configuración local:

- `admin@demo.local` — administrador
- `docente@demo.local` — docente con cursos asignados
- `docente.sin.cursos@demo.local` — docente sin cursos
- `docente.inactivo@demo.local` — docente inactivo
- `estudiante@demo.local` — estudiante con matrícula y calificaciones de demostración
- `estudiante.sin.matricula@demo.local` — estudiante sin cursos matriculados

La institución es Universidad Marco Porras. El sistema se llama Sistema de matrícula, el logotipo es UMP, el periodo de referencia es I semestre 2026 y la escala es Nota de 0 a 100. Los cursos de demostración conservan los códigos `2026-1` y `2026-2`. Esas carreras, cursos y notas no son un catálogo real.

## Historias de esta etapa

HU01, HU02, HU09, HU10, HU11 y HU12 ya están implementadas, con persistencia y con la autorización resuelta en el servidor. HU05 sirve para que el docente vea sus cursos asignados; no incluye la administración de listas ni de notas. HU03, HU04, HU06, HU07 y HU08 siguen pendientes, y el menú lo indica.
