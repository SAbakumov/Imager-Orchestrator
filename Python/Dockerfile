FROM python:3.11

WORKDIR /dag_orchestrator 

COPY ./requirements.txt /dag_orchestrator/requirements.txt

RUN apt-get update && apt-get install -y \
    build-essential \
    libfreetype6-dev \
    libpng-dev \
    pkg-config \
 && rm -rf /var/lib/apt/lists/*

RUN pip install --no-cache-dir  -r /dag_orchestrator/requirements.txt

COPY ./app /dag_orchestrator/app

COPY ./imagedata /dag_orchestrator/imagedata

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8400"]

EXPOSE 8400
