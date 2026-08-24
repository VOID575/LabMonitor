FROM node:20-slim AS base
WORKDIR /app
COPY package*.json ./
RUN npm install

FROM base AS development
EXPOSE 4200
CMD ["npx", "ng", "serve", "--host", "0.0.0.0", "--poll", "2000"]

FROM base AS build
COPY . .
RUN npm run build --configuration=production

FROM nginx:alpine AS production
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/LabInterface/browser /usr/share/nginx/html
EXPOSE 80