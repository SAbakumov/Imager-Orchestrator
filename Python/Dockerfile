FROM python:3.11 
 

WORKDIR /dag_orchestrator 

COPY ./requirements.txt /dag_orchestrator/requirements.txt

RUN apt-get update && apt-get install -y \
    build-essential \
    libfreetype6-dev \
    libpng-dev \
    pkg-config \
 && rm -rf /var/lib/apt/lists/*

RUN curl -fsSL https://code-server.dev/install.sh | sh

RUN pip install --no-cache-dir  -r /dag_orchestrator/requirements.txt

COPY .vscode /dag_orchestrator/.vscode

COPY ./app /dag_orchestrator/app

COPY ./imagedata /dag_orchestrator/imagedata

CMD ["sh", "-c", "\
  code-server --bind-addr 0.0.0.0:8080 --auth none  & \
  python3 -m debugpy --listen 0.0.0.0:5678 -m uvicorn app.main:app --host 0.0.0.0 --port 8400 --reload \
"]
EXPOSE 8400 8080

RUN code-server --install-extension ms-python.python
