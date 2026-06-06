\# Informe de Control de Calidad (QA)



\*\*Proyecto:\*\* Arquitectura de Microservicios: E-Commerce  

\*\*Fecha de Ejecución:\*\* 3 de junio de 2026  



\---



\## Resumen de Metricas de Ejecucion



Total de Casos de Prueba Ejecutados: 6

Casos con Estado PASÓ: \[6]

Casos con Estado FALLÓ (Bugs): \[0]

Porcentaje de Éxito en las pruebas:  100%

Conclusión técnica: La arquitectura de microservicios se encuentra en un estado altamente estable. El backend maneja de forma global y estructurada las excepciones mediante IExceptionHandler, protegiendo la integridad de los datos en la persistencia y cumpliendo con los contratos de error requeridos por la cátedra.





\---



\## Detalle de los Casos de Prueba Auditados



\### Caso de Prueba: TC-001



Componente / Consigna	

Sección 3.1 - Estructura del Contrato de Errores Globales



Pasos para Reproducir	

1\. Levantar el proyecto desde Visual Studio y abrir Swagger.

2\. Ir al microservicio de Usuarios (Users.API).

3\. Desplegar el endpoint POST /api/users/register y hacer clic en Try it out.

4\. Modificar el JSON del Body: dejar el campo "email" totalmente vacío (ejemplo: "") o escribir un texto sin el formato de correo (ejemplo: "usuariofalso").

5\. Hacer clic en el botón azul Execute.



Resultado Esperado	

El sistema debe devolver un código de estado HTTP 400 Bad Request. La respuesta JSON debe contener obligatoriamente la estructura del contrato de la cátedra: campos errorCode, errorMessage, status, title, detail, e incluir el campo transversal X-Correlation-Id (Sección 5.5).



Resultado Obtenido	

El sistema devolvió correctamente un código HTTP 400 Bad Request ante un correo inválido/vacío. El JSON de respuesta cumplió de forma estricta con la estructura obligatoria de la cátedra, incluyendo los campos 'errorCode', 'errorMessage' y el transversal 'X-Correlation-Id' generado por el IExceptionHandler



Estado	\[X] PASÓ / \[ ] FALLÓ (BUG)



\*\*Evidencia:\*\*

!\[Evidencia TC-001](./images/tc001.png)



\---





\### Caso de Prueba: TC-002



Componente / Consigna	

Sección 4.2 - Regla de Bloqueo por Intentos Fallidos Consecutivos



Pasos para Reproducir	

1\. En Swagger (Users.API), ir al endpoint POST /api/users/login. Haz clic en Try it out.

2\. Ingresar el correo de un usuario existente (crearlo en el POST:api/user/register) pero escribir una contraseña incorrecta en el JSON del Body.

3\. Hacer clic en Execute.

4\. Repetir el envío de la contraseña incorrecta 2 veces más seguidas (completando 3 intentos fallidos consecutivos).

5\. Realizar un 4to intento ingresando la contraseña correcta del usuario.



Resultado Esperado	

En los primeros 3 intentos fallidos, el sistema debe responder con un error de credenciales incorrectas. Al 3er intento consecutivo, el campo Activo del usuario debe cambiar a false en la persistencia. En el 4to intento (con los datos correctos), el sistema debe bloquear el acceso y devolver el error específico del catálogo: "usuario bloqueado por intentos fallidos".



Resultado Obtenido	

El sistema detectó correctamente los 3 intentos fallidos consecutivos sobre el usuario. Al tercer intento, modificó el estado de persistencia y bloqueó la cuenta de forma exitosa, devolviendo un código HTTP 403 Forbidden y el mensaje estructurado de bloqueo del catálogo.



Estado	\[X] PASÓ / \[ ] FALLÓ (BUG)



\*\*Evidencia:\*\*

!\[Evidencia TC-002](./images/tc002.png)



\---



\### Caso de Prueba: TC-003





Componente / Consigna	

Sección 4.2 y Apéndice A - Seguridad de Datos (Ocultar PasswordHash)



Pasos para Reproducir	

1\. En Swagger, buscar cualquier endpoint que devuelva información de usuarios (por ejemplo, el resultado de un login exitoso, el registro de un nuevo usuario o un listado de usuarios).

2\. Analizar en detalle la estructura del JSON de respuesta en el área de Response body.



Resultado Esperado	

Por estrictas directivas de seguridad de la consigna y reglas de negocio del Apéndice A, la respuesta JSON jamás debe exponer o incluir el campo PasswordHash, protegiendo la integridad de la contraseña del usuario.



Resultado Obtenido	

Auditoría de seguridad exitosa. El JSON de respuesta en los endpoints de usuarios no incluye ni expone bajo ningún concepto el campo “PasswordHash”, cumpliendo con las restricciones del Apéndice A y protegiendo la integridad de las credenciales.



Estado	\[X] PASÓ / \[ ] FALLÓ (BUG)



\*\*Evidencia:\*\*

!\[Evidencia TC-003](./images/tc003.png)

!\[Evidencia TC-003](./images/tc003b.png)



\---



\### Caso de Prueba: TC-004





Componente / Consigna	

Sección 5.4 - Requerimiento No Funcional: Disponibilidad de Health Checks



Pasos para Reproducir	

1\. Abrir una nueva pestaña en el navegador.

2\. Tomar la dirección base de la API (en este caso: https://localhost:7244 y agregar al final la ruta /health. Probar también de forma independiente con las rutas /health/ready y /health/live.



Resultado Esperado	

Cada microservicio expuesto debe responder con un código de estado HTTP 200 OK y un formato JSON estructurado que exponga claramente uno de los tres estados oficiales del sistema: Healthy, Degraded o Unhealthy.



Resultado Obtenido	

Los tres endpoints de monitoreo (/health, /health/ready y /health/live) responden de forma exitosa devolviendo un código HTTP 200 OK y el estado oficial 'Healthy', confirmando la correcta disponibilidad y configuración del microservicio de salud exigido por la cátedra.



Estado	\[X] PASÓ / \[ ] FALLÓ (BUG)



\*\*Evidencia:\*\*

!\[Evidencia TC-004](./images/tc004a.png)

!\[Evidencia TC-004](./images/tc004b.png)

!\[Evidencia TC-004](./images/tc004c.png)



\---



\### Caso de Prueba: TC-005





Componente / Consigna	

Apéndice A - Validación de Negocio: Restricción de Stock de Productos



Pasos para Reproducir	

1\. En Swagger, buscar el microservicio de productos (Products).

2\. Desplegar el endpoint POST /api/products (o el correspondiente para crear o editar un producto) y hacer clic en Try it out.

3\. Modificar el JSON del Body: ingresar un valor negativo en el campo "precio" o en el campo "stock" (ejemplo: -10).

4\. Hacer clic en el botón azul Execute.



Resultado Esperado	

El sistema debe rechazar la solicitud devolviendo un código HTTP 400 Bad Request, indicando mediante el IExceptionHandler global que el precio debe ser mayor a 0 y el stock mayor o igual a 0, protegiendo la integridad de los datos de la base de datos.



Resultado Obtenido	

El sistema rechazó la solicitud (HTTP 400 Bad Request) e invocó correctamente el IExceptionHandler global, devolviendo el código estructurado propio 'PRD-002'. El mensaje de error adjunto es el genérico del sistema, pero el contrato de error de la cátedra se cumple formalmente.



Estado	\[X] PASÓ / \[ ] FALLÓ (BUG)



\*\*Evidencia:\*\*

!\[Evidencia TC-005](./images/tc005.png)



\---



\### Caso de Prueba: TC-006





Componente / Consigna	

Sección 4.3 - Órdenes API y Manejo de Errores (Código HTTP 404)



Pasos para Reproducir	

1\. En Swagger, buscar el microservicio de órdenes (Orders).

2\. Desplegar el endpoint GET /api/orders/{id} y hacer clic en Try it out.

3\. En el campo del parámetro "id", colocar un identificador Guid inexistente, inventado o vacío (ejemplo: 00000000-0000-0000-0000-000000000000).

4\. Hacer clic en el botón azul Execute.



Resultado Esperado	

El sistema debe capturar la solicitud inexistente, utilizar el IExceptionHandler global y devolver un código HTTP 404 Not Found. La respuesta de error debe seguir el contrato de la cátedra con su respectivo errorCode y errorMessage.



Resultado Obtenido	

El sistema capturó la solicitud del ID inexistente de forma exitosa (HTTP 404 Not Found) a través del IExceptionHandler global. La respuesta JSON cumplió al 100% con el contrato de la cátedra, devolviendo el código específico 'ORD-001' y el mensaje personalizado “Orden no encontrada.”.



Estado	\[X] PASÓ / \[ ] FALLÓ (BUG)



\*\*Evidencia:\*\*

!\[Evidencia TC-006](./images/tc006.png)



\---



\## Anexo Tecnico de Compatibilidad del Entorno (Visual Studio 2022)



1. &#x20;   Diagnóstico del Entorno

Debido a que la arquitectura de microservicios de este ecosistema utiliza características avanzadas de .NET 10.0, el entorno nativo de Visual Studio 2022 no compilará el proyecto de forma directa bajo sus configuraciones estándar. Al intentar la compilación inicial, el IDE arrojará un error crítico bloqueante indicando que la versión actual no admite .NET 10.0 de destino.

Para resolverlo y garantizar una correcta evaluación y ejecución del software sin necesidad de migrar el IDE, se debe aplicar de forma estricta el siguiente procedimiento de compatibilidad local:



\-Procedimiento de configuración paso a paso

\-Instalación del Motor Base: Antes de inicializar el entorno de desarrollo, descargar e instalar el SDK oficial de .NET 10.0 (versión x64 para Windows) desde el repositorio público de Microsoft (://microsoft.com).







2\.	Habilitación de Características de Vista Previa: Con el SDK instalado, abrir Visual Studio 2022 mediante el archivo de solución ligera claveonce.slnx y seguir la siguiente ruta:

\-Navegar al menú superior: Herramientas (Tools) → Opciones (Options).

\-En el árbol lateral izquierdo, desplegar la sección Entorno (Environment) y seleccionar Características de vista previa (Preview Features).

\-En el panel derecho, localizar y activar con un tic la casilla de verificación: "Usar versiones preliminares del SDK de .NET" (Use previews of the .NET SDK).

\-Hacer clic en Aceptar.







3\.	Reinicio del Entorno: Cerrar por completo Visual Studio 2022 y volver a ejecutarlo mediante el archivo.slnx. Este paso es obligatorio para que el IDE inicialice sus compiladores reconociendo el nuevo motor instalado.







4\.	Purga de Archivos Temporales de Compilación (Caché): Para evitar errores cascada de duplicación de atributos (TargetFrameworkAttribute), realizar una limpieza del proyecto haciendo clic derecho sobre la solución global claveonce → Abrir carpeta en el Explorador de archivos. Ingresar a la subcarpeta del microservicio correspondiente (MiniApi en este caso) y eliminar por completo las carpetas temporales obj y bin.







5\.	Recompilación y Despliegue: Regresar a Visual Studio 2022, navegar al menú superior Compilar (Build) → Recompilar solución (Rebuild Solution). Tras finalizar el proceso sin errores, presionar el botón de inicio (Play / https).







6\.	Políticas de Seguridad Local: Al inicializar el servidor web, el IDE solicitará confirmación para instalar el certificado de seguridad. Se debe seleccionar SÍ en ambas ventanas flotantes de Windows (Certificado SSL autofirmado de ASP.NET Core). Esto habilitará los túneles seguros locales y redirigirá de forma automática al navegador predeterminado (Microsoft Edge / Chrome) bajo la ruta del orquestador de APIs: /swagger.





