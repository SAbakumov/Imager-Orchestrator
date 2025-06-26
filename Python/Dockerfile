FROM python:3.10

WORKDIR /dag_orchestrator 

COPY ./requirements.txt /dag_orchestrator/requirements.txt

RUN pip install --no-cache-dir --upgrade -r /dag_orchestrator/requirements.txt

COPY ./app /dag_orchestrator/app

CMD ["uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "80"]

EXPOSE 80
