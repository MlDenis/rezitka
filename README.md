# rezitka

Перезапуск проекта для выката изменений в коде: 
docker-compose up --force-recreate --build


version: '3.4'

services:
  postgresqlmonitoringbot:
    container_name: bot
    image: ${DOCKER_REGISTRY-}postgresqlmonitoringbot
    build:
      context: .
      dockerfile: Dockerfile
    tty: true
    links:
    - db

  db:
    container_name: db
    image: postgres:latest
    restart: always
    ports:
     - "5432:5432"
    environment:
     - POSTGRES_DB=TestDb
     - POSTGRES_USER=postgres
     - POSTGRES_PASSWORD=Qwerty123

  smoldb:
    container_name: smoldb
    image: postgres:latest
    restart: always
    ports:
     - "5433:5432"
    environment:
     - POSTGRES_DB=TestDb
     - POSTGRES_USER=postgres
     - POSTGRES_PASSWORD=Qwerty123
