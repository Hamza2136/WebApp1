var dtable;
$(document).ready(function () {
    dtable = $('#mytable').DataTable({
        "ajax": {
            "url": "/Admin/Order/AllOrders"
        },
        "columns": [
            { "data": 'name' },
            { "data": 'phone' },
            { "data": 'orderStatus' },
            { "data": 'orderTotal' },
            {
                "data": "id",
                "render": function (data) {
                    return `
                    <a href="/admin/order/orderdetails?id=${data}"><i class="bi bi-pencil-square"></i></a>
                    `;
                }
            }
        ]
    })
})
