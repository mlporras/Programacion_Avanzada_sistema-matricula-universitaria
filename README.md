# Sistema de matrícula universitaria

Aplicación ASP.NET Core MVC del avance 2. Incluye autenticación con Identity, gestión de usuarios y oferta académica, matrícula, consulta personal de calificaciones y gestión de estudiantes por el docente.

## Requisitos y ejecución

El proyecto utiliza .NET 8 y el SDK 8.0.425, fijado en global.json. Las dependencias de Identity y Entity Framework Core son de la versión 8.0.11. La conexión de ejemplo utiliza SQL Server Express con autenticación de Windows.

Abra ProyectoFinal_PorrasMarco.sln en Visual Studio. Configure DefaultConnection con su base y defina Seed:AdminPassword y Seed:ContrasenaDemostracion mediante user-secrets o variables de entorno.

Desde esta carpeta:

```text
dotnet restore
dotnet build ProyectoFinal_PorrasMarco.sln
dotnet run --project ProyectoFinal_PorrasMarco --launch-profile http
```

La aplicación aplica las migraciones al iniciar. El registro público crea únicamente cuentas de estudiantes.

## Base de datos y acceso

El respaldo MatriculaUniversitaria.bak contiene esquema y datos de demostración. Se creó y restauró con SQL Server 2025 Express 17.0.1000.7, por lo que no puede restaurarse en versiones anteriores.

Para usar contraseñas propias después de restaurar, defina las dos claves y active Seed:RestablecerContrasenasSinteticas durante una sola ejecución. Después, devuelva el valor a false. El procedimiento solo actualiza las seis cuentas de demostración.

En el ZIP, las instrucciones están en ../BaseDatos/Restauracion.md y ../Documentacion/Guia_instalacion.md. Las cuentas principales son admin@demo.local, docente@demo.local y estudiante@demo.local. Las contraseñas se configuran localmente y no se incluyen en el proyecto.

## Funciones implementadas

Están completas HU01, HU02, HU06, HU09, HU10, HU11 y HU12. HU05 está parcial, faltan los accesos funcionales a calificaciones y reportes. HU03, HU04, HU07 y HU08 continúan pendientes.

El administrador gestiona usuarios, carreras y cursos. El docente agrega o remueve estudiantes de sus cursos con justificación. El estudiante elige carrera, consulta la oferta por periodo, administra su matrícula y revisa sus notas.

Repositorio: https://github.com/mlporras/Programacion_Avanzada_sistema-matricula-universitaria
